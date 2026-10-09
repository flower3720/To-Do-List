# Regression suite (ASCII-only source).
# Chinese expectations are read from expect.json as UTF-8, and request bodies are
# written as UTF-8 files, to avoid Windows PowerShell 5.1 ANSI mis-decoding.
$ErrorActionPreference = 'Stop'

$base = 'http://127.0.0.1:5099'
# 以脚本所在目录为基准，保证可移植
$tmpDir = $PSScriptRoot
$logPath = Join-Path $tmpDir 'regression.log'
$bodyFile = Join-Path $tmpDir 'body.json'
$expectFile = Join-Path $tmpDir 'expect.json'
# 项目目录（用于重置数据库）
$projectDir = Join-Path (Split-Path -Parent $PSScriptRoot) 'TodoApi'

Start-Transcript -Path $logPath -Force | Out-Null

$script:pass = 0
$script:fail = 0

function Check($name, $cond, $extra) {
  if ($cond) { Write-Output "[PASS] $name"; $script:pass++ }
  else { Write-Output "[FAIL] $name :: $extra"; $script:fail++ }
}

$E = [System.IO.File]::ReadAllText($expectFile, [System.Text.Encoding]::UTF8) | ConvertFrom-Json

function SendRaw($method, $path, $json) {
  [System.IO.File]::WriteAllText($bodyFile, $json, (New-Object System.Text.UTF8Encoding($false)))
  return (& curl.exe -s -X $method "$base$path" -H 'Content-Type: application/json; charset=utf-8' --data-binary "@$bodyFile")
}

function SendJson($method, $path, $json) {
  return ((SendRaw $method $path $json) | ConvertFrom-Json)
}

function SendCode($method, $path, $json) {
  [System.IO.File]::WriteAllText($bodyFile, $json, (New-Object System.Text.UTF8Encoding($false)))
  return (& curl.exe -s -o NUL -w '%{http_code}' -X $method "$base$path" -H 'Content-Type: application/json; charset=utf-8' --data-binary "@$bodyFile")
}

function GetRaw($path) {
  return (& curl.exe -s "$base$path")
}

function GetJson($path) {
  return ((GetRaw $path) | ConvertFrom-Json)
}

function HttpCode($method, $path) {
  return (& curl.exe -s -o NUL -w '%{http_code}' -X $method "$base$path")
}

# ---------------- reset database to a known baseline ----------------
# MigrateAsync on startup recreates the schema and the HasData seed rows.
foreach ($f in 'todo.db', 'todo.db-shm', 'todo.db-wal') {
  $p = Join-Path $projectDir $f
  if (Test-Path $p) { Remove-Item $p -Force }
}
Write-Output 'database reset to fresh state'

# ---------------- start server ----------------
$srv = Start-Job -ScriptBlock {
  param($cliHome, $nuget, $proj, $logDir)
  $env:DOTNET_CLI_HOME = $cliHome
  $env:NUGET_PACKAGES = $nuget
  $env:ASPNETCORE_URLS = 'http://127.0.0.1:5099'
  $env:ASPNETCORE_ENVIRONMENT = 'Development'
  Set-Location $proj
  dotnet run --no-build --no-launch-profile *>&1 | Out-File (Join-Path $logDir 'server.log') -Encoding utf8
} -ArgumentList 'H:\DSH\.dotnet-cli-home', 'H:\DSH\.nuget-packages', $projectDir, $tmpDir

$ready = $false
for ($i = 0; $i -lt 60; $i++) {
  Start-Sleep -Milliseconds 1000
  try { if ((Invoke-WebRequest "$base/api/health" -UseBasicParsing -TimeoutSec 3).StatusCode -eq 200) { $ready = $true; break } } catch {}
}

try {
  if (-not $ready) {
    Write-Output 'SERVER NOT READY'
    Get-Content (Join-Path $tmpDir 'server.log') -ErrorAction SilentlyContinue | Select-Object -Last 30
    exit 1
  }
  Write-Output ('server ready after {0}s' -f ($i + 1))

  # ---------------- unified envelope ----------------
  $health = GetJson '/api/health'
  Check 'health returns unified envelope' ($health.code -eq 0 -and $health.data.status -eq 'healthy') ($health | ConvertTo-Json -Compress)
  Check 'envelope has timestamp' ($null -ne $health.timestamp) ('timestamp=' + $health.timestamp)

  $doc = (GetRaw '/swagger/v1/swagger.json') | ConvertFrom-Json
  $paths = @($doc.paths.PSObject.Properties.Name)
  Write-Output ('swagger paths: ' + ($paths -join ', '))
  $expectedPaths = @('/api/Todos', '/api/Todos/{id}', '/api/Todos/completed', '/api/Todos/stats',
                     '/api/Categories', '/api/Categories/{id}', '/api/health')
  $missing = @($expectedPaths | Where-Object { $paths -notcontains $_ })
  Check 'swagger exposes all 7 paths' ($missing.Count -eq 0) ($missing -join ',')
  $todoOps = @($doc.paths.'/api/Todos'.PSObject.Properties.Name)
  Check 'todo list path has GET + POST' ($todoOps.Count -eq 2) ($todoOps -join ',')

  # ---------------- categories ----------------
  $cats = GetJson '/api/categories'
  Check 'category seed = 3' ($cats.data.Count -eq 3) $cats.data.Count
  Check 'category carries todoCount' (($cats.data | Where-Object { $_.todoCount -gt 0 }).Count -ge 1) ($cats.data | ConvertTo-Json -Compress)
  Write-Output ('categories: ' + (($cats.data | ForEach-Object { $_.name + '=' + $_.todoCount }) -join ', '))

  $newCat = SendJson 'POST' '/api/categories' ('{"name":"' + $E.categoryName + '","sortOrder":9}')
  Check 'create category ok' ($newCat.code -eq 0 -and $newCat.data.name -eq $E.categoryName) ($newCat | ConvertTo-Json -Compress)
  $catId = $newCat.data.id

  $dupJson = '{"name":"' + $E.categoryName + '","sortOrder":9}'
  $dupCat = SendJson 'POST' '/api/categories' $dupJson
  Check 'duplicate category rejected (1004)' ($dupCat.code -eq 1004) ($dupCat | ConvertTo-Json -Compress)
  Check 'duplicate category http 409' ((SendCode 'POST' '/api/categories' $dupJson) -eq '409') ''

  $updCat = SendJson 'PUT' "/api/categories/$catId" ('{"name":"' + $E.categoryUpdated + '","sortOrder":5}')
  Check 'update category ok' ($updCat.code -eq 0 -and $updCat.data.name -eq $E.categoryUpdated) ($updCat | ConvertTo-Json -Compress)
  # Read-back checks: asserting only the response body misses "detached entity never persisted" bugs
  $catReadBack = GetJson "/api/categories/$catId"
  Check 'category rename persisted (read-back)' ($catReadBack.data.name -eq $E.categoryUpdated) ('read-back=' + $catReadBack.data.name)
  $catList = GetJson '/api/categories'
  $catInList = $catList.data | Where-Object { $_.id -eq $catId }
  Check 'category rename visible in list' ($catInList.name -eq $E.categoryUpdated) ('list=' + $catInList.name)

  # ---------------- todos ----------------
  $list = GetJson '/api/todos?pageSize=100'
  Check 'todo seed = 4' ($list.data.totalCount -eq 4) $list.data.totalCount
  Check 'list items carry categoryName' (($list.data.items | Where-Object { $_.categoryName }).Count -ge 3) $list.data.totalCount
  $first = $list.data.items[0]
  Write-Output ('first item: ' + $first.title + ' | cat=' + $first.categoryName + ' | pri=' + $first.priorityText + ' | overdue=' + $first.isOverdue)

  $createJson = '{"title":"' + $E.todoTitlePadded + '","description":"' + $E.todoDesc + '","priority":2,"categoryId":' + $catId + ',"dueDate":"2026-11-01T00:00:00"}'
  $created = SendJson 'POST' '/api/todos' $createJson
  Check 'create todo ok' ($created.code -eq 0 -and $created.data.id -gt 4) ($created | ConvertTo-Json -Compress)
  Check 'title trimmed' ($created.data.title -eq $E.todoTitle) ('[' + $created.data.title + ']')
  Check 'description utf8 roundtrip' ($created.data.description -eq $E.todoDesc) ('[' + $created.data.description + ']')
  Check 'created todo carries categoryName' ($created.data.categoryName -eq $E.categoryUpdated) ('got=' + $created.data.categoryName)
  $todoId = $created.data.id

  $badCat = SendJson 'POST' '/api/todos' '{"title":"x","categoryId":99999}'
  Check 'unknown category rejected (1001)' ($badCat.code -eq 1001) ($badCat | ConvertTo-Json -Compress)

  $invalid = SendJson 'POST' '/api/todos' '{"title":""}'
  Check 'validation error returns 1001' ($invalid.code -eq 1001) ($invalid | ConvertTo-Json -Compress)
  Check 'validation error http 400' ((SendCode 'POST' '/api/todos' '{"title":""}') -eq '400') ''

  $notFound = GetJson '/api/todos/99999'
  Check 'missing todo returns 1003' ($notFound.code -eq 1003) ($notFound | ConvertTo-Json -Compress)
  Check 'missing todo http 404' ((HttpCode 'GET' '/api/todos/99999') -eq '404') ''

  $single = GetJson "/api/todos/$todoId"
  Check 'get by id ok' ($single.code -eq 0 -and $single.data.id -eq $todoId) ($single | ConvertTo-Json -Compress)

  $patched = SendJson 'PATCH' "/api/todos/$todoId" '{"isCompleted":true}'
  Check 'patch sets completed' ($patched.data.isCompleted -eq $true) ($patched | ConvertTo-Json -Compress)
  Check 'patch keeps other fields' ($patched.data.title -eq $E.todoTitle -and $patched.data.priority -eq 2) ($patched | ConvertTo-Json -Compress)

  # 把待办留在测试分类下（categoryId 不变），便于后面校验改名可见性
  $updJson = '{"title":"' + $E.todoTitleUpdated + '","description":null,"isCompleted":false,"priority":0,"categoryId":' + $catId + '}'
  $updated = SendJson 'PUT' "/api/todos/$todoId" $updJson
  Check 'put updates all fields' ($updated.data.title -eq $E.todoTitleUpdated -and $updated.data.priority -eq 0 -and $updated.data.categoryName -eq $E.categoryUpdated) ($updated | ConvertTo-Json -Compress)

  # ---------------- filter / paging ----------------
  $byCat = GetJson "/api/todos?categoryId=$catId&pageSize=100"
  Check 'filter by category' (($byCat.data.items | Where-Object { $_.categoryId -ne $catId }).Count -eq 0) $byCat.data.totalCount

  $byPri = GetJson '/api/todos?priority=2&sortBy=title&desc=true&pageSize=100'
  Check 'filter by priority + sort' (($byPri.data.items | Where-Object { $_.priority -ne 2 }).Count -eq 0) $byPri.data.totalCount

  $paged = GetJson '/api/todos?page=1&pageSize=2'
  Check 'paging returns 2 items' ($paged.data.items.Count -eq 2 -and $paged.data.pageSize -eq 2) $paged.data.items.Count
  Check 'paging totalCount is full count' ($paged.data.totalCount -eq 5) $paged.data.totalCount

  $clamped = GetJson '/api/todos?page=1&pageSize=9999'
  Check 'pageSize clamped to 100' ($clamped.data.pageSize -eq 100) $clamped.data.pageSize

  $kwEncoded = [System.Uri]::EscapeDataString($E.keyword)
  $keyword = GetJson ('/api/todos?keyword=' + $kwEncoded)
  Check 'keyword search hits' ($keyword.data.totalCount -ge 1) ($E.keyword + ' -> ' + $keyword.data.totalCount)

  # ---------------- stats ----------------
  $stats = GetJson '/api/todos/stats'
  Check 'stats arithmetic consistent' ($stats.data.total -eq ($stats.data.completed + $stats.data.active)) ($stats.data | ConvertTo-Json -Compress)
  $seedCount = $stats.data.byCategory.($E.seedCategory)
  Check 'stats has byCategory' ($null -ne $seedCount) ($stats.data.byCategory | ConvertTo-Json -Compress)
  Write-Output ('stats: total=' + $stats.data.total + ' completed=' + $stats.data.completed + ' overdue=' + $stats.data.overdue)
  Write-Output ('byCategory: ' + ($stats.data.byCategory | ConvertTo-Json -Compress))

  # ---------------- FK guard ----------------
  $fkCode = HttpCode 'DELETE' '/api/categories/1'
  $fkResp = (GetRaw '/api/categories/1') | Out-Null
  $fkResp = (SendRaw 'DELETE' '/api/categories/1' '') | ConvertFrom-Json
  Check 'category with todos cannot be deleted' ($fkResp.code -eq 1004 -and $fkCode -eq '409') ('HTTP=' + $fkCode + ' code=' + $fkResp.code)
  Write-Output ('conflict message: ' + $fkResp.message)

  # ---------------- change-tracker staleness regression ----------------
  # Guards two classic EF Core traps:
  #  1) a detached (AsNoTracking) entity is modified but never persisted;
  #  2) Include'd navigation data served from a stale change-tracker cache.
  $renamed = SendJson 'PUT' "/api/categories/$catId" ('{"name":"' + $E.renamedCategory + '","sortOrder":7}')
  Check 'rename to new name ok' ($renamed.data.name -eq $E.renamedCategory) ('got=' + $renamed.data.name)
  $catAfter = GetJson "/api/categories/$catId"
  Check 'rename persisted in category table' ($catAfter.data.name -eq $E.renamedCategory) ('read-back=' + $catAfter.data.name)
  $todosAfter = GetJson ('/api/todos?categoryId=' + $catId + '&pageSize=100')
  $names = @($todosAfter.data.items | ForEach-Object { $_.categoryName } | Select-Object -Unique)
  Check 'rename visible to todo query (no stale cache)' ($names.Count -eq 1 -and $names[0] -eq $E.renamedCategory) ($names -join ',')

  # ---------------- delete ----------------
  $deleted = (SendRaw 'DELETE' "/api/todos/$todoId" '') | ConvertFrom-Json
  Check 'delete returns envelope' ($deleted.code -eq 0 -and $deleted.data.id -eq $todoId) ($deleted | ConvertTo-Json -Compress)
  Check 'deleted todo http 200 not 204' ((HttpCode 'DELETE' "/api/todos/$todoId") -ne '204') ''
  Check 'deleted todo now 404' ((HttpCode 'GET' "/api/todos/$todoId") -eq '404') ''

  $delCat = (SendRaw 'DELETE' "/api/categories/$catId" '') | ConvertFrom-Json
  Check 'delete empty category ok' ($delCat.code -eq 0) ($delCat | ConvertTo-Json -Compress)

  $cleared = (SendRaw 'DELETE' '/api/todos/completed' '') | ConvertFrom-Json
  Check 'clear completed returns count' ($cleared.code -eq 0 -and $cleared.data -ge 1) ($cleared | ConvertTo-Json -Compress)

  $final = GetJson '/api/todos?pageSize=100'
  Check 'seed data back to 3 after cleanup' ($final.data.totalCount -eq 3) $final.data.totalCount

  Write-Output ''
  Write-Output ('=========== PASS=' + $script:pass + ' FAIL=' + $script:fail + ' ===========')
}
finally {
  Stop-Job $srv -ErrorAction SilentlyContinue
  Remove-Job $srv -Force -ErrorAction SilentlyContinue
  Stop-Transcript | Out-Null
}

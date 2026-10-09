# Minimal repro: category rename visibility (ASCII source; Chinese from expect.json)
$ErrorActionPreference = 'Stop'
$base = 'http://127.0.0.1:5099'
$tmpDir = 'H:\DSH\.run'
$bodyFile = Join-Path $tmpDir 'body.json'
$logPath = Join-Path $tmpDir 'repro.log'
Start-Transcript -Path $logPath -Force | Out-Null

$E = [System.IO.File]::ReadAllText((Join-Path $tmpDir 'expect.json'), [System.Text.Encoding]::UTF8) | ConvertFrom-Json

function Show($label, $text) {
  Write-Output "----- $label -----"
  Write-Output $text
}

function SendRaw($method, $path, $json) {
  [System.IO.File]::WriteAllText($bodyFile, $json, (New-Object System.Text.UTF8Encoding($false)))
  $code = & curl.exe -s -o (Join-Path $tmpDir 'resp.json') -w '%{http_code}' -X $method "$base$path" -H 'Content-Type: application/json; charset=utf-8' --data-binary "@$bodyFile"
  $body = [System.IO.File]::ReadAllText((Join-Path $tmpDir 'resp.json'), [System.Text.Encoding]::UTF8)
  return "HTTP $code`n$body"
}

function GetRaw($path) {
  $code = & curl.exe -s -o (Join-Path $tmpDir 'resp.json') -w '%{http_code}' "$base$path"
  $body = [System.IO.File]::ReadAllText((Join-Path $tmpDir 'resp.json'), [System.Text.Encoding]::UTF8)
  return "HTTP $code`n$body"
}

# fresh database
foreach ($f in 'todo.db', 'todo.db-shm', 'todo.db-wal') {
  $p = Join-Path 'H:\DSH\TodoApi' $f
  if (Test-Path $p) { Remove-Item $p -Force }
}

$srv = Start-Job -ScriptBlock {
  $env:DOTNET_CLI_HOME = 'H:\DSH\.dotnet-cli-home'
  $env:NUGET_PACKAGES = 'H:\DSH\.nuget-packages'
  $env:ASPNETCORE_URLS = 'http://127.0.0.1:5099'
  $env:ASPNETCORE_ENVIRONMENT = 'Development'
  Set-Location 'H:\DSH\TodoApi'
  dotnet run --no-build --no-launch-profile *>&1 | Out-File 'H:\DSH\.run\server.log' -Encoding utf8
}

$ready = $false
for ($i = 0; $i -lt 60; $i++) {
  Start-Sleep -Milliseconds 1000
  try { if ((Invoke-WebRequest "$base/api/health" -UseBasicParsing -TimeoutSec 3).StatusCode -eq 200) { $ready = $true; break } } catch {}
}

try {
  if (-not $ready) { Write-Output 'NOT READY'; exit 1 }
  Write-Output ('ready after {0}s' -f ($i + 1))

  Show '1. create category' (SendRaw 'POST' '/api/categories' ('{"name":"' + $E.categoryName + '","sortOrder":9}'))
  $catId = ((( & curl.exe -s "$base/api/categories") | ConvertFrom-Json).data | Where-Object { $_.name -eq $E.categoryName }).id
  Write-Output "catId = $catId"

  Show '2. rename category' (SendRaw 'PUT' "/api/categories/$catId" ('{"name":"' + $E.categoryUpdated + '","sortOrder":5}'))

  Show '3. GET category list' (GetRaw '/api/categories')

  Show '4. GET single category' (GetRaw "/api/categories/$catId")

  Show '5. create todo in category' (SendRaw 'POST' '/api/todos' ('{"title":"' + $E.todoTitle + '","priority":1,"categoryId":' + $catId + '}'))

  Show '6. GET single todo' (GetRaw "/api/todos/$catId")

  Show '7. list todos filtered by category' (GetRaw ('/api/todos?categoryId=' + $catId + '&pageSize=100'))

  Show '8. rename again to renamedCategory' (SendRaw 'PUT' "/api/categories/$catId" ('{"name":"' + $E.renamedCategory + '","sortOrder":7}'))

  Show '9. list todos filtered by category (after rename)' (GetRaw ('/api/todos?categoryId=' + $catId + '&pageSize=100'))
}
finally {
  Stop-Job $srv -ErrorAction SilentlyContinue
  Remove-Job $srv -Force -ErrorAction SilentlyContinue
  Stop-Transcript | Out-Null
}

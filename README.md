# 待办事项管理系统 API（TodoApi）

> 大学生课程设计示例项目 —— 基于 **C# / ASP.NET Core 8 + Entity Framework Core + SQLite** 的 RESTful Web API。

采用 `Controllers / Services / Repositories / Entities / DTOs` 五层结构，使用 **Code First + Migration**
管理数据库结构与种子数据，**Fluent API** 配置一对多关系与索引，**AutoMapper** 完成实体与 DTO 映射，
全局异常中间件 + `ApiResult<T>` 统一所有接口的返回格式。

---

## 一、技术栈

| 类别 | 技术 | 说明 |
| --- | --- | --- |
| 语言 | C# 12 | .NET 8 默认语言版本 |
| 框架 | ASP.NET Core 8 Web API | 控制器风格 + 依赖注入 |
| ORM | EF Core | Code First + Migration + Fluent API |
| 数据库 | SQLite | 文件型数据库，免安装 |
| 对象映射 | AutoMapper 13 | Profile 集中配置映射规则 |
| 接口文档 | Swashbuckle (Swagger) | 加载 XML 注释，可在线调试 |

---

## 二、目录结构

```
TodoApi.sln                        解决方案
TodoApi/
├── TodoApi.csproj                 项目文件（目标框架与 NuGet 依赖）
├── Program.cs                     入口：注册服务、应用迁移、配置中间件管道
├── appsettings.json               配置（连接字符串、日志级别）
├── Properties/launchsettings.json 本地调试配置（端口 5099）
│
├── Models/                        实体层（Entities）
│   ├── Category.cs                分类实体（一对多的“一”端）
│   └── TodoItem.cs                待办实体（“多”端，含 Priority 枚举）
│
├── Data/
│   └── TodoDbContext.cs           数据库上下文：Fluent API 配置关系/索引/HasData
│
├── Migrations/                    EF Core 迁移（由 dotnet ef 生成）
│   ├── 2026..._InitialCreate.cs
│   ├── 2026..._InitialCreate.Designer.cs
│   └── TodoDbContextModelSnapshot.cs
│
├── Repositories/                  仓储层：只负责“怎么取数据”
│   ├── IRepository.cs             泛型仓储接口
│   ├── Repository.cs              泛型仓储实现
│   ├── TodoRepository.cs          待办专用仓储（组合查询、排序、统计）
│   └── CategoryRepository.cs      分类专用仓储
│
├── Services/                      业务层：只负责业务规则与协调
│   ├── ITodoService.cs / TodoService.cs
│   └── ICategoryService.cs / CategoryService.cs
│
├── Dtos/                          数据传输对象
│   ├── TodoDtos.cs                新增/更新/局部更新/查询/响应/统计
│   └── CategoryDtos.cs
│
├── Mapping/
│   └── MappingProfile.cs          AutoMapper 映射配置
│
├── Middleware/
│   ├── ExceptionHandlingMiddleware.cs            全局异常处理
│   └── ExceptionHandlingMiddlewareExtensions.cs  注册扩展
│
├── Exceptions/
│   └── AppException.cs            业务异常体系（404/409/400）
│
├── Common/
│   ├── ApiResult.cs               统一返回格式
│   ├── ApiCode.cs                 业务状态码常量
│   ├── PagedResult.cs             分页结果包装
│   └── PagingHelper.cs            分页参数规范化
│
├── Controllers/
│   ├── TodosController.cs         待办接口（7 个端点）
│   └── CategoriesController.cs    分类接口
│
└── wwwroot/index.html             演示前端页面

tests/
├── regression.ps1                 全接口回归测试（45 项断言）
├── expect.json                    中文期望值（UTF-8，供脚本读取）
└── repro-category-rename.ps1      分类改名可见性的最小复现脚本
```

**分层职责**：Controller 只处理 HTTP 路由与状态码 → Service 执行业务规则 →
Repository 封装数据访问 → DbContext 与数据库交互。任何一层都不越权。

---

## 三、运行环境

- Windows 10/11、macOS 或 Linux
- [.NET 8 SDK](https://dotnet.microsoft.com/download)（`dotnet --version` 显示 8.0 或更高即可）

> 项目目标框架为 `net8.0`，配套依赖为 EF Core 8.0.31，运行时只需 .NET 8 运行库。

---

## 四、如何运行

在解决方案根目录（`TodoApi.sln` 所在目录）执行：

```bash
dotnet restore          # 首次运行需联网还原 NuGet 包
dotnet run --project TodoApi
```

启动时会**自动应用迁移**（建库 + 写入种子数据），控制台输出：

```
info: 检测到 1 个待应用的迁移：20261009..._InitialCreate
info: 数据库迁移完成，当前数据库：todo.db
Now listening on: http://localhost:5099
```

浏览器访问：

| 地址 | 说明 |
| --- | --- |
| http://localhost:5099/swagger | **Swagger 接口文档（可在线测试）** |
| http://localhost:5099/ | 演示网页（含分类筛选、分页、统计） |
| http://localhost:5099/api/health | 健康检查 |

Visual Studio 2022 直接打开 `TodoApi.sln` 按 `F5` 亦可。

### 数据库迁移（dotnet-ef 工具）

本仓库的 `.config/dotnet-tools.json` 已声明 `dotnet-ef` 工具。首次使用需还原工具：

```bash
dotnet tool restore

# 修改实体后新增迁移
dotnet ef migrations add 迁移名称 --project TodoApi

# 应用迁移（正常启动也会自动应用）
dotnet ef database update --project TodoApi

# 生成建表 SQL 脚本（用于交付/上生产）
dotnet ef migrations script --project TodoApi -o schema.sql
```

---

## 五、数据模型与一对多关系

```
Category (1) ──────< TodoItem (N)
```

| Category（分类） | 类型 | 说明 |
| --- | --- | --- |
| Id | int | 主键，自增 |
| Name | string(50) | 名称，**唯一索引** |
| SortOrder | int | 排序值 |
| CreatedAt | DateTime | 创建时间 |
| TodoItems | ICollection | 导航属性（“一”端） |

| TodoItem（待办） | 类型 | 说明 |
| --- | --- | --- |
| Id | int | 主键，自增 |
| Title | string(100) | 标题，必填 |
| Description | string(500) | 描述，可空 |
| IsCompleted | bool | 是否完成 |
| Priority | int（枚举） | 0=低，1=中，2=高 |
| DueDate | DateTime? | 截止时间 |
| CreatedAt / UpdatedAt | DateTime | 服务端生成 |
| CategoryId | int? | 外键，可空（表示未分类） |
| Category | Category? | 导航属性（“多”端） |

### Fluent API 配置（`TodoDbContext.OnModelCreating`）

```csharp
// 一对多关系 + 外键 + 删除行为
entity.HasOne(e => e.Category)
      .WithMany(c => c.TodoItems)
      .HasForeignKey(e => e.CategoryId)
      .HasConstraintName("FK_TodoItems_Categories_CategoryId")
      .OnDelete(DeleteBehavior.Restrict);   // 分类下有数据时不允许删除

// 唯一索引：分类名不重复
entity.HasIndex(e => e.Name).IsUnique();

// 组合索引：列表页最高频的“按分类 + 完成状态”查询
entity.HasIndex(e => new { e.CategoryId, e.IsCompleted });

// 种子数据：由迁移写入，数据库状态可复现
entity.HasData(new Category { Id = 1, Name = "课程学习", ... });
```

生成的 DDL：

```sql
CONSTRAINT "FK_TodoItems_Categories_CategoryId" FOREIGN KEY ("CategoryId")
    REFERENCES "Categories" ("Id") ON DELETE RESTRICT
CREATE UNIQUE INDEX "IX_Categories_Name" ON "Categories" ("Name");
CREATE INDEX "IX_TodoItems_CategoryId_IsCompleted" ON "TodoItems" ("CategoryId","IsCompleted");
CREATE INDEX "IX_TodoItems_IsCompleted" ON "TodoItems" ("IsCompleted");
CREATE INDEX "IX_TodoItems_Priority" ON "TodoItems" ("Priority");
```

> **为什么用 `Restrict` 而不是 `Cascade`？** 如果级联删除，删除一个分类会静默删掉其下所有待办，
> 属于高风险操作。`Restrict` 让数据库直接拒绝，业务层据此返回 `409` 并提示用户先转移数据。
> 外键约束 + 业务校验双重保障。

---

## 六、接口说明

**所有接口统一返回 `ApiResult` 结构：**

```json
{
  "code": 0,
  "message": "success",
  "data": { },
  "timestamp": "2026-01-01T10:00:00",
  "traceId": "0HN..."
}
```

| 字段 | 说明 |
| --- | --- |
| code | 业务状态码，**0 表示成功**；失败为错误码 |
| message | 提示信息 |
| data | 业务数据，失败时为 null |
| timestamp | 服务器时间 |
| traceId | 链路追踪 Id，可与服务端日志对照排查 |

**业务状态码**（`Common/ApiCode.cs`）：

| code | 含义 | 对应 HTTP |
| --- | --- | --- |
| 0 | 成功 | 200 |
| 1001 | 参数校验失败（含传了不存在的分类 Id） | 400 |
| 1002 | 业务规则不满足 | 400 |
| 1003 | 资源不存在 | 404 |
| 1004 | 资源冲突（名称重复、分类下仍有待办） | 409 |
| 5000 | 服务器内部错误 | 500 |

> 采用「**HTTP 状态码 + 业务码双层表达**」：HTTP 状态码供网关、监控、缓存等基础设施识别，
> 业务码供前端区分具体失败原因。

### 待办接口 `/api/todos`

| 方法 | 路径 | 说明 |
| --- | --- | --- |
| GET | `/api/todos` | 分页查询（关键字/状态/优先级/分类筛选 + 排序） |
| GET | `/api/todos/{id}` | 查询单条 |
| POST | `/api/todos` | 新增 |
| PUT | `/api/todos/{id}` | 整体更新 |
| PATCH | `/api/todos/{id}` | 局部更新（只改提交的字段） |
| DELETE | `/api/todos/{id}` | 删除（返回被删除的数据） |
| DELETE | `/api/todos/completed` | 清空已完成 |
| GET | `/api/todos/stats` | 统计信息 |

**列表查询参数**：

| 参数 | 默认 | 说明 |
| --- | --- | --- |
| keyword | 无 | 标题/描述模糊搜索 |
| isCompleted | 无 | 完成状态筛选 |
| priority | 无 | 优先级筛选（0/1/2） |
| categoryId | 无 | 分类筛选 |
| sortBy | createdAt | createdAt / dueDate / priority / title / category |
| desc | false | 是否降序 |
| page | 1 | 页码 |
| pageSize | 10 | 每页条数，**超过 100 会被限制为 100** |

**响应示例**：

```json
{
  "code": 0,
  "message": "success",
  "data": {
    "items": [
      {
        "id": 1,
        "title": "完成《Web 应用开发》课程设计报告",
        "description": "包含需求分析、系统设计、接口说明与测试记录。",
        "isCompleted": false,
        "priority": 2,
        "priorityText": "高",
        "dueDate": "2026-01-20T00:00:00",
        "createdAt": "2026-01-01T09:00:00",
        "updatedAt": null,
        "categoryId": 1,
        "categoryName": "课程学习",
        "isOverdue": false
      }
    ],
    "totalCount": 4,
    "page": 1,
    "pageSize": 10,
    "totalPages": 1
  },
  "timestamp": "2026-01-01T10:00:00",
  "traceId": null
}
```

### 分类接口 `/api/categories`

| 方法 | 路径 | 说明 |
| --- | --- | --- |
| GET | `/api/categories` | 全部分类（含各分类待办数量） |
| GET | `/api/categories/{id}` | 单个分类 |
| POST | `/api/categories` | 新增（名称重复返回 1004） |
| PUT | `/api/categories/{id}` | 更新 |
| DELETE | `/api/categories/{id}` | 删除（下有待办返回 1004） |

**请求示例**：

```json
// POST /api/todos
{ "title": "复习数据结构", "description": "二叉树遍历", "priority": 2, "categoryId": 1, "dueDate": "2026-02-01T00:00:00" }

// PATCH /api/todos/5  只改一个字段
{ "isCompleted": true }

// POST /api/categories
{ "name": "实习求职", "sortOrder": 3 }
```

**失败响应示例**：

```json
{ "code": 1003, "message": "待办事项（Id=99999）不存在", "data": null, "timestamp": "...", "traceId": "..." }
{ "code": 1004, "message": "分类“课程学习”下仍有待办事项，请先移除或转移后再删除", ... }
{ "code": 1001, "message": "标题不能为空；标题长度必须在 1~100 个字符之间", ... }
```

---

## 七、关键设计说明（答辩要点）

1. **分层与依赖注入**：`Program.cs` 统一注册 `DbContext`、仓储、Service，全部使用 `AddScoped`。
   Scoped 表示「每个 HTTP 请求一个实例」——DbContext 与依赖它的仓储、Service 必须同生命周期，
   否则会出现连接被多线程共享的问题。AutoMapper 的 `IMapper` 是线程安全的，注册为单例。

2. **泛型仓储 + 专用仓储**：`IRepository<T>` 收敛通用 CRUD 与分页代码；
   `ITodoRepository` / `ICategoryRepository` 承载各自特有的组合查询。Service 完全不接触 `DbContext`。
   > 注意：EF Core 的 `DbSet` 本身已是仓储 + 工作单元，若只做纯透传包装就是画蛇添足。
   > 本项目的仓储层只承载**查询关注点**（`IQueryable` 组合、排序、`AsNoTracking`），因此有存在价值。

3. **AutoMapper 与 DTO 安全**：映射规则集中在 `MappingProfile`。请求 DTO 只映射允许客户端提交的字段，
   `Id`、`CreatedAt`、`UpdatedAt` 显式 `Ignore()` 由服务端赋值，**从根本上防止客户端篡改**。
   PATCH 请求不做整体映射——它的语义是"只改提交的字段"，由服务层逐字段判断 `null` 后赋值，
   避免未提交字段被默认值覆盖。

4. **统一返回格式 + 全局异常处理**：业务代码只抛 `NotFoundException` / `ConflictException` 等语义异常，
   由 `ExceptionHandlingMiddleware` 统一转成 `ApiResult` 并映射 HTTP 状态码，
   Controller 里**没有任何 try/catch**。未预期异常记 Error 日志（含堆栈）并返回通用提示，避免泄露实现细节。

5. **异步全链路**：仓储与 Service 全部 `async/await`，数据库操作使用 `ToListAsync()` / `CountAsync()` /
   `SaveChangesAsync()`，并逐层传递 `CancellationToken`（客户端断开连接时可及时取消查询）。

6. **日志**：Service 层用 `ILogger<T>` 记录结构化日志，例如
   `"新增待办 | Id={Id} Title={Title} CategoryId={CategoryId}"`，
   参数化占位符可在日志系统中按字段检索。业务异常记 Warning，未预期异常记 Error。

7. **迁移与种子数据**：表结构由 Migration 管理，种子数据用 `HasData` 写在迁移里，
   因此**数据库状态可复现**、可审计。启动时 `MigrateAsync()` 自动应用未执行的迁移。

8. **分页与性能**：先 `CountAsync` 取总数再 `Skip/Take` 取当前页，只读查询一律 `AsNoTracking()`，
   `pageSize` 强制上限 100，避免一次查询过多数据。

---

## 八、开发中踩到的坑（真实问题，可写进报告）

### 坑 1：设计时执行导致迁移建表冲突

`dotnet ef` 生成迁移时会**执行 `Program.cs`**（为了构造 `DbContext`）。由于启动逻辑里有
`MigrateAsync()`，结果在生成迁移前就把表建好了，随后 `database update` 报
`SQLite Error 1: 'table "TodoItems" already exists'`。

**解决**：加设计时守卫，`dotnet ef` 运行时跳过迁移：

```csharp
var isDesignTime = AppDomain.CurrentDomain.GetAssemblies()
    .Any(a => a.GetName().Name is "dotnet-ef" or "Microsoft.EntityFrameworkCore.Design");
if (!isDesignTime) { /* MigrateAsync */ }
```

### 坑 2：无跟踪查询导致修改"看起来成功、实际没保存"

分类改名接口返回了 200 和新名称，但**重新查询仍是旧名称**。
根因是当时用 `AsNoTracking()` 的查询取出实体——它是**游离（detached）**的，
EF 不跟踪它的变化，`SaveChangesAsync()` 不会生成任何 UPDATE 语句，只有响应里的内存对象被改了。

**解决**：区分「只读查询」与「待修改查询」，写操作必须用带跟踪的查询：

```csharp
// 只读：无跟踪，避免变更跟踪器缓存导致的脏读
public async Task<Category?> GetWithItemsAsync(...) => await Query().Include(...).FirstOrDefaultAsync(...);

// 写操作：带跟踪，否则 SaveChangesAsync 不生效
public async Task<Category?> GetForUpdateAsync(...) => await Set.FirstOrDefaultAsync(...);
```

### 坑 3：只断言响应体会漏掉持久化 bug

上面那个 bug 之所以没被及时发现，是因为测试只校验了**响应体**——而响应体恰好返回了被修改的内存对象。
**教训**：写操作后必须**回读校验**（重新 GET 一次确认已落库）。回归脚本现已覆盖此场景。

### 坑 4：Windows PowerShell 5.1 发送中文变成 `???`

用 `Invoke-RestMethod` 提交中文，服务端收到 `??????`。排查后确认是**客户端问题**：
Windows PowerShell 5.1 默认用非 UTF-8 编码序列化请求体。改用 `curl.exe --data-binary` 后往返正常。
本项目回归脚本因此统一采用「UTF-8 文件承载请求体 + curl 发送」的方式。

---

## 九、测试

`tests/regression.ps1` 是覆盖全部接口的回归脚本（**45 项断言**，当前全部通过），
每次运行会先把数据库重置到已知基线，因此可重复执行：

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File tests\regression.ps1
```

覆盖范围：统一返回格式与 traceId、Swagger 路径完整性、分类 CRUD（含重名 409）、
待办 CRUD、参数校验 400、资源不存在 404、分类下有待办时删除 409、一对多导航属性、
按分类/优先级筛选、排序、分页与 pageSize 上限、关键字搜索、统计聚合、
中文 UTF-8 往返、**改名可见性（防脏读回归）**。

`tests/repro-category-rename.ps1` 是分类改名可见性的最小复现脚本，
可逐步打印每个请求的完整响应，便于定位问题。

---

## 十、数据库说明

- 数据库文件 `TodoApi/todo.db`，首次运行由迁移自动创建。
- 想恢复到初始状态：停止程序，删除 `todo.db`（连同 `-shm`、`-wal`），重新 `dotnet run`。
- 生产环境建议把连接字符串放到环境变量或密钥管理服务，而不是提交到代码仓库。

---

## 十一、常见问题

**Q1：端口被占用，提示 `Failed to bind to address http://localhost:5099`？**
修改 `Properties/launchsettings.json` 的 `applicationUrl`。

**Q2：`dotnet restore` 无法连接 nuget.org？**
需联网还原 NuGet 包；校园网代理环境请先配置代理。

**Q3：修改实体后数据库没变化？**
`MigrateAsync()` 只应用**已生成的迁移**。改完实体要执行 `dotnet ef migrations add 名称` 生成新迁移。

**Q4：`dotnet ef` 报 "table already exists"？**
见第八节坑 1，检查设计时守卫是否生效。

**Q5：为什么 `DELETE` 返回 200 而不是 204？**
因为采用统一 `ApiResult` 返回格式，`204 No Content` 没有响应体可承载 `code`/`message`。
这是「统一格式」与「严格 REST 语义」之间的取舍，本项目选择前者，便于前端用一套逻辑解析所有响应。

---

## 十二、可扩展方向

- **认证授权**：ASP.NET Core Identity + JWT，实现用户级数据隔离（当前项目未做，属于已知不足）。
- **单元测试**：xUnit + Moq 测试 Service 层，`WebApplicationFactory` 做集成测试。
- **FluentValidation**：替换 DataAnnotations，支持更复杂的跨字段业务校验。
- **Redis 缓存**：缓存统计类接口；`IHttpClientFactory` 调用外部服务。
- **容器化与 CI**：编写 Dockerfile，用 GitHub Actions 实现提交自动构建 + 跑测试。

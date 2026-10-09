using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using TodoApi.Common;
using TodoApi.Data;
using TodoApi.Mapping;
using TodoApi.Middleware;
using TodoApi.Repositories;
using TodoApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

builder.Services.AddControllers();

builder.Services.Configure<Microsoft.AspNetCore.Mvc.ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var message = string.Join("；", context.ModelState
            .SelectMany(kv => kv.Value?.Errors ?? new())
            .Select(e => e.ErrorMessage)
            .Where(m => !string.IsNullOrWhiteSpace(m)));

        var body = ApiResult.Fail(ApiCode.ValidationError,
            string.IsNullOrWhiteSpace(message) ? "请求参数不合法" : message);
        body.TraceId = context.HttpContext.TraceIdentifier;

        return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(body);
    };
});

var connectionString = builder.Configuration.GetConnectionString("Default")
                       ?? "Data Source=todo.db";
builder.Services.AddDbContext<TodoDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<ITodoRepository, TodoRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();

builder.Services.AddScoped<ITodoService, TodoService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "待办事项管理系统 API",
        Version = "v1",
        Description = "ASP.NET Core 9 + EF Core + SQLite 的待办事项 Web API。"
    });

    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

var app = builder.Build();

var isDesignTime = AppDomain.CurrentDomain.GetAssemblies()
    .Any(a => a.GetName().Name is "dotnet-ef" or "Microsoft.EntityFrameworkCore.Design");

if (!isDesignTime)
{
    using var scope = app.Services.CreateScope();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<TodoDbContext>();

    await db.Database.MigrateAsync();
    logger.LogInformation("数据库迁移完成：{Database}", db.Database.GetDbConnection().Database);
}

app.UseGlobalExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "TodoApi v1");
    options.DocumentTitle = "待办事项管理系统 API";
});

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("AllowAll");

app.MapControllers();

app.MapGet("/api/health", () => Results.Ok(ApiResult<object>.Ok(new
{
    name = "TodoApi",
    version = "2.0.0",
    status = "healthy",
    time = DateTime.Now
})));

app.Run();

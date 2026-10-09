using Microsoft.AspNetCore.Mvc;
using TodoApi.Common;
using TodoApi.Dtos;
using TodoApi.Services;

namespace TodoApi.Controllers;

/// <summary>待办事项接口。</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class TodosController : ControllerBase
{
    private readonly ITodoService _todoService;

    public TodosController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    /// <summary>
    /// 分页查询待办列表，支持关键字搜索、状态筛选、优先级筛选、分类筛选和排序。
    /// </summary>
    /// <response code="200">查询成功，返回分页结果。</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<PagedResult<TodoResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResult<PagedResult<TodoResponse>>>> GetList(
        [FromQuery] TodoQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _todoService.GetListAsync(query, cancellationToken);
        return Ok(ApiResult<PagedResult<TodoResponse>>.Ok(result));
    }

    /// <summary>
    /// 根据 Id 查询单条待办。
    /// </summary>
    /// <response code="200">查询成功。</response>
    /// <response code="404">记录不存在。</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResult<TodoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResult<TodoResponse>>> GetById(int id, CancellationToken cancellationToken)
    {
        var item = await _todoService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResult<TodoResponse>.Ok(item));
    }

    /// <summary>
    /// 新增待办。
    /// </summary>
    /// <response code="200">创建成功，data 为新建的记录。</response>
    /// <response code="400">参数校验失败。</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<TodoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ApiResult<TodoResponse>>> Create(
        [FromBody] TodoCreateRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _todoService.CreateAsync(request, cancellationToken);
        return Ok(ApiResult<TodoResponse>.Ok(created, "创建成功"));
    }

    /// <summary>
    /// 整体更新待办，未提交的字段会被覆盖为默认值。
    /// </summary>
    /// <response code="200">更新成功。</response>
    /// <response code="400">参数校验失败。</response>
    /// <response code="404">记录不存在。</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResult<TodoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResult<TodoResponse>>> Update(
        int id,
        [FromBody] TodoUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _todoService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResult<TodoResponse>.Ok(updated, "更新成功"));
    }

    /// <summary>
    /// 局部更新待办，只修改请求体中出现的字段。
    /// </summary>
    /// <response code="200">更新成功。</response>
    /// <response code="400">参数校验失败。</response>
    /// <response code="404">记录不存在。</response>
    [HttpPatch("{id:int}")]
    [ProducesResponseType(typeof(ApiResult<TodoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResult<TodoResponse>>> Patch(
        int id,
        [FromBody] TodoPatchRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _todoService.PatchAsync(id, request, cancellationToken);
        return Ok(ApiResult<TodoResponse>.Ok(updated, "更新成功"));
    }

    /// <summary>
    /// 删除待办，返回被删除的记录。
    /// </summary>
    /// <response code="200">删除成功，data 为被删除的记录。</response>
    /// <response code="404">记录不存在。</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResult<TodoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResult<TodoResponse>>> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _todoService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResult<TodoResponse>.Ok(deleted, "删除成功"));
    }

    /// <summary>
    /// 清空所有已完成的待办。
    /// </summary>
    /// <response code="200">返回本次删除的条数。</response>
    [HttpDelete("completed")]
    [ProducesResponseType(typeof(ApiResult<int>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResult<int>>> ClearCompleted(CancellationToken cancellationToken)
    {
        var removed = await _todoService.ClearCompletedAsync(cancellationToken);
        return Ok(ApiResult<int>.Ok(removed, $"已清理 {removed} 条已完成的待办事项"));
    }

    /// <summary>
    /// 获取统计数据（总数、完成率、逾期数量、按分类分布等）。
    /// </summary>
    /// <response code="200">统计成功。</response>
    [HttpGet("stats")]
    [ProducesResponseType(typeof(ApiResult<TodoStatsResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResult<TodoStatsResponse>>> GetStats(CancellationToken cancellationToken)
    {
        var stats = await _todoService.GetStatsAsync(cancellationToken);
        return Ok(ApiResult<TodoStatsResponse>.Ok(stats));
    }
}

using Microsoft.AspNetCore.Mvc;
using TodoApi.Common;
using TodoApi.Dtos;
using TodoApi.Services;

namespace TodoApi.Controllers;

/// <summary>分类接口。</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>
    /// 查询全部分类，包含每个分类下的待办数量。
    /// </summary>
    /// <response code="200">查询成功。</response>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResult<List<CategoryResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResult<List<CategoryResponse>>>> GetAll(CancellationToken cancellationToken)
    {
        var categories = await _categoryService.GetAllAsync(cancellationToken);
        return Ok(ApiResult<List<CategoryResponse>>.Ok(categories));
    }

    /// <summary>
    /// 根据 Id 查询单个分类。
    /// </summary>
    /// <response code="200">查询成功。</response>
    /// <response code="404">分类不存在。</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(ApiResult<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResult<CategoryResponse>>> GetById(int id, CancellationToken cancellationToken)
    {
        var category = await _categoryService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResult<CategoryResponse>.Ok(category));
    }

    /// <summary>
    /// 新增分类。
    /// </summary>
    /// <response code="200">创建成功。</response>
    /// <response code="400">参数校验失败。</response>
    /// <response code="409">分类名称已存在。</response>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResult<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResult<CategoryResponse>>> Create(
        [FromBody] CategoryRequest request,
        CancellationToken cancellationToken)
    {
        var created = await _categoryService.CreateAsync(request, cancellationToken);
        return Ok(ApiResult<CategoryResponse>.Ok(created, "创建成功"));
    }

    /// <summary>
    /// 更新分类。
    /// </summary>
    /// <response code="200">更新成功。</response>
    /// <response code="404">分类不存在。</response>
    /// <response code="409">分类名称与其他分类重复。</response>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(ApiResult<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResult<CategoryResponse>>> Update(
        int id,
        [FromBody] CategoryRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await _categoryService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResult<CategoryResponse>.Ok(updated, "更新成功"));
    }

    /// <summary>
    /// 删除分类。分类下仍有待办事项时不允许删除。
    /// </summary>
    /// <response code="200">删除成功，data 为被删除的分类。</response>
    /// <response code="404">分类不存在。</response>
    /// <response code="409">分类下仍有待办事项。</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(typeof(ApiResult<CategoryResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResult), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ApiResult<CategoryResponse>>> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _categoryService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResult<CategoryResponse>.Ok(deleted, "删除成功"));
    }
}

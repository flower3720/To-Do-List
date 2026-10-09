using AutoMapper;
using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Mapping;

/// <summary>实体与 DTO 的映射配置。</summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // 请求 DTO -> 实体，只映射允许客户端提交的字段
        CreateMap<TodoCreateRequest, TodoItem>()
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title.Trim()))
            .ForMember(d => d.Description, opt => opt.MapFrom(s =>
                string.IsNullOrWhiteSpace(s.Description) ? null : s.Description.Trim()))
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.IsCompleted, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.Category, opt => opt.Ignore());

        CreateMap<TodoUpdateRequest, TodoItem>()
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title.Trim()))
            .ForMember(d => d.Description, opt => opt.MapFrom(s =>
                string.IsNullOrWhiteSpace(s.Description) ? null : s.Description.Trim()))
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.CreatedAt, opt => opt.Ignore())
            .ForMember(d => d.UpdatedAt, opt => opt.Ignore())
            .ForMember(d => d.Category, opt => opt.Ignore());

        // PATCH 由服务层逐字段判断后赋值，这里不做整体映射

        // 实体 -> 响应 DTO
        CreateMap<TodoItem, TodoResponse>()
            .ForMember(d => d.Priority, opt => opt.MapFrom(s => (int)s.Priority))
            .ForMember(d => d.PriorityText, opt => opt.MapFrom(s => GetPriorityText(s.Priority)))
            .ForMember(d => d.CategoryName, opt => opt.MapFrom(s => s.Category != null ? s.Category.Name : null))
            .ForMember(d => d.IsOverdue, opt => opt.MapFrom(s =>
                !s.IsCompleted && s.DueDate.HasValue && s.DueDate.Value.Date < DateTime.Today));

        CreateMap<Category, CategoryResponse>()
            .ForMember(d => d.TodoCount, opt => opt.MapFrom(s => s.TodoItems.Count));
    }

    private static string GetPriorityText(Priority priority) => priority switch
    {
        Priority.Low => "低",
        Priority.High => "高",
        _ => "中"
    };
}

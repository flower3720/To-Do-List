using AutoMapper;
using TodoApi.Dtos;
using TodoApi.Models;

namespace TodoApi.Mapping;

/// <summary>实体与 DTO 的映射配置。</summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
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

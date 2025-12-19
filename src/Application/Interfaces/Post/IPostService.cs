using Application.DTOs.Post;

namespace Application.Interfaces;

/// <summary>
///     Định nghĩa các phương thức xử lý liên quan đến bài viết
/// </summary>
public interface IPostService
{
    Task<PagedResultDto<PostDto>> GetPagedAsync(PostQueryParametersDto parameters);
    Task<PostDto?> GetByIdAsync(Guid id);
    Task<PostDto> CreatePostAsync(PostCreateDto post);
    Task<PostDto?> UpdatePostAsync(Guid id, PostUpdateDto post);
    Task<bool> DeletePostAsync(Guid id);
}
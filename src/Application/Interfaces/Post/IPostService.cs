using Application.DTOs.Post;

namespace Application.Interfaces;

public interface IPostService
{
    Task<PagedResultDto<PostDto>> GetPagedAsync(PostQueryParametersDto parameters);
    Task<PostDto?> GetByIdAsync(Guid id);
    Task<PostDto> CreatePostAsync(PostCreateDto post);
    Task<PostDto?> UpdatePostAsync(Guid postId, Guid userId, bool isAdmin, PostUpdateDto dto);
    Task<bool> DeletePostAsync(Guid postId, Guid userId, bool isAdmin);
}
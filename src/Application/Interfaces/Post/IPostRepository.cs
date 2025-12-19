using Application.DTOs.Post;
using Domain.Entities;

namespace Application.Interfaces;

public interface IPostRepository
{
    Task<PagedResultDto<Post>> GetPagedAsync(PostQueryParametersDto parameters);
    Task<Post?> GetByIdAsync(Guid postId);
    Task<Post> CreateAsync(Post post);
    Task<Post?> UpdateAsync(Post post);
    Task<bool> DeleteAsync(Guid postId);
}
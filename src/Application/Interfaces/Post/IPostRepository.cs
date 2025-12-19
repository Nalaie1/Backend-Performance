using Application.DTOs.Post;
using Domain.Entities;

namespace Application.Interfaces;

public interface IPostRepository
{
    Task<PagedResultDto<Post>> GetPagedAsync(PostQueryParametersDto parameters);
    Task<Post?> GetByIdAsync(Guid PostId);
    Task<Post> CreateAsync(Post post);
    Task<Post?> UpdateAsync(Guid PostId, string NewContent);
    Task<bool> DeleteAsync(Guid PostId);
}
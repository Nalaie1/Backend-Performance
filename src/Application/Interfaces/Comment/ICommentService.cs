using Application.DTOs.Comment;

namespace Application.Interfaces;

/// <summary>
///     Định nghĩa các phương thức thao tác với bình luận
/// </summary>
public interface ICommentService
{
    Task<List<CommentDto>> GetCommentTreeAsync(Guid postId);
    Task<List<CommentFlattenDto>> GetCommentFlattenAsync(Guid postId);
    Task<CommentDto> CreateCommentAsync(CommentCreateDto dto);
    Task<CommentDto?> UpdateCommentAsync(Guid id, CommentUpdateDto dto);
    Task<bool> DeleteCommentAsync(Guid id);
}
using Domain.Entities;

namespace Application.Interfaces;

public interface ICommentRepository
{
    // ===================== READ =====================
    Task<List<Comment>> GetAllCommentsForPost(Guid postId, bool includeReplies = true);
    Task<List<Comment>> GetAllCommentsRecursive(Guid postId);
    Task<List<Comment>> GetAllCommentsIterative(Guid postId);
    // ===================== CREATE =====================
    Task<Comment> CreateAsync(Comment comment);
    // ===================== UPDATE =====================
    Task<Comment?> UpdateAsync(Guid commentId, string newContent);
    // ===================== DELETE =====================
    Task<bool> DeleteAsync(Guid commentId);
}
using Domain.Entities;

namespace Application.Interfaces;

public interface ICommentRepository
{
    Task<List<Comment>> GetAllCommentsForPost(Guid postId, bool includeReplies = true);
}
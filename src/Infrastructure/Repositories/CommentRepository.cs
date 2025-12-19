using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class CommentRepository : ICommentRepository
{
    private readonly AppDbContext _context;

    public CommentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Comment?> GetByIdAsync(Guid commentId)
    {
        return await _context.Comments.FindAsync(commentId);
    }

    /// <summary>
    ///     Lấy tất cả bình luận của một bài viết, có thể bao gồm hoặc không bao gồm câu trả lời
    ///     Lọc những bình luận cấp cao (ParentCommentId == null)
    /// </summary>
    public async Task<List<Comment>> GetAllCommentsForPost(Guid postId, bool includeReplies = true)
    {
        if (includeReplies)
            return await _context.Comments
                .Where(c => c.PostId == postId && c.ParentCommentId == null)
                .Include(c => c.User)
                .Include(c => c.Replies)
                .ThenInclude(r => r.User)
                .ToListAsync();

        return await _context.Comments
            .Where(c => c.PostId == postId && c.ParentCommentId == null)
            .Include(c => c.User)
            .ToListAsync();
    }


    /// <summary>
    ///     Lấy tất cả bình luận của một bài viết bằng CTE recursive (nếu có hỗ trợ)
    ///     Hoặc fallback về recursive code nếu dùng InMemory database (cho tests)
    ///     Trả về flat list tất cả bình luận (Giống SQL CTE)
    /// </summary>
    public async Task<List<Comment>> GetAllCommentsRecursive(Guid postId)
    {
        // Kiểm tra nếu đang dùng InMemory database (cho tests)
        if (_context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
        {
            // Fallback: Load tất cả và collect bằng recursive code
            // Trả về flat list tất cả comments (giống SQL CTE)
            var allComments = await _context.Comments
                .Where(c => c.PostId == postId)
                .Include(c => c.User)
                .ToListAsync();

            var result = new List<Comment>();
            var topLevelComments = allComments.Where(c => c.ParentCommentId == null).ToList();

            // Collect tất cả comments bằng recursive traversal
            foreach (var topComment in topLevelComments) CollectCommentsRecursive(topComment, allComments, result);

            return result;
        }

        // SQL Server: dùng CTE recursive
        var sql = @"
            WITH CommentCTE AS (
                SELECT * FROM Comments WHERE PostId = {0} AND ParentCommentId IS NULL
                UNION ALL
                SELECT c.* FROM Comments c
                INNER JOIN CommentCTE p ON c.ParentCommentId = p.Id
            )
            SELECT * FROM CommentCTE";
        return await _context.Comments.FromSqlRaw(sql, postId).ToListAsync();
    }

    // ===================== CREATE =====================
    public async Task<Comment> CreateAsync(Comment comment)
    {
        comment.Id = Guid.NewGuid();
        // comment.CreatedAt = DateTime.UtcNow;
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    // ===================== UPDATE =====================
    public async Task<Comment?> UpdateAsync(Guid commentId, string newContent)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null) return null;

        comment.Content = newContent;
        // comment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return comment;
    }

    // ===================== DELETE =====================
    public async Task<bool> DeleteAsync(Guid commentId)
    {
        var comment = await _context.Comments
            .Include(c => c.Replies)
            .FirstOrDefaultAsync(c => c.Id == commentId);

        if (comment == null)
            return false;

        if (comment.Replies.Any())
            _context.Comments.RemoveRange(comment.Replies);

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return true;
    }

    /// <summary>
    ///     Lấy tất cả bình luận của một bài viết bằng cách load tất cả một lần và xây dựng cây bình luận
    ///     Trả về danh sách các bình luận cấp cao (có chứa replies bên trong)
    /// </summary>
    public async Task<List<Comment>> GetAllCommentsIterative(Guid postId)
    {
        // Load tất cả comments của post một lần
        var allComments = await _context.Comments
            .Where(c => c.PostId == postId)
            .Include(c => c.User)
            .ToListAsync();
    
        // Tạo dictionary để lookup nhanh: parentId -> list of children
        var childrenDict = allComments
            .Where(c => c.ParentCommentId != null)
            .GroupBy(c => c.ParentCommentId!.Value)
            .ToDictionary(g => g.Key, g => g.ToList());
    
        // Tìm tất cả top-level comments
        var topLevelComments = allComments.Where(c => c.ParentCommentId == null).ToList();
    
        // Build tree structure iteratively bằng queue (không đệ quy)
        var queue = new Queue<Comment>();
        foreach (var topComment in topLevelComments) queue.Enqueue(topComment);
    
        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
    
            // Tìm và thêm replies từ dictionary (O(1) lookup)
            if (childrenDict.TryGetValue(current.Id, out var replies))
                foreach (var reply in replies)
                {
                    current.Replies.Add(reply);
                    queue.Enqueue(reply);
                }
        }
    
        return topLevelComments;
    }

    private void CollectCommentsRecursive(Comment comment, List<Comment> allComments, List<Comment> result)
    {
        result.Add(comment); // Thêm comment vào flat list

        var replies = allComments.Where(c => c.ParentCommentId == comment.Id).ToList();
        foreach (var reply in replies) CollectCommentsRecursive(reply, allComments, result); // Đệ quy
    }
}
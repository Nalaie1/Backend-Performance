using Application.Interfaces.Upload;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class UploadRepository : IUploadRepository
{
    private readonly AppDbContext _context;

    public UploadRepository(AppDbContext context)
    {
        _context = context;
    }

    // Set user avatar URL
    public async Task<bool> SetUserAvatarAsync(Guid userId, string url)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null) return false;
        user.AvatarUrl = url;
        await _context.SaveChangesAsync();
        return true;
    }

    // Set post image URL
    public async Task<bool> SetPostImageUrlAsync(Guid postId, string url)
    {
        var post = await _context.Posts.FirstOrDefaultAsync(x => x.Id == postId);
        if (post == null) return false;
        post.ImageUrl = url;
        await _context.SaveChangesAsync();
        return true;
    }

    // Set comment attachment URL
    public async Task<bool> SetCommentAttachmentUrlAsync(Guid commentId, string url)
    {
        var comment = await _context.Comments.FirstOrDefaultAsync(x => x.Id == commentId);
        if (comment == null) return false;
        comment.AttachmentUrl = url;
        await _context.SaveChangesAsync();
        return true;
    }
}
namespace Application.Interfaces.Upload;

public interface IUploadRepository
{
    Task<bool> SetUserAvatarAsync(Guid userId, string url);
    Task<bool> SetPostImageUrlAsync(Guid postId, string url);
    Task<bool> SetCommentAttachmentUrlAsync(Guid commentId, string url);
}
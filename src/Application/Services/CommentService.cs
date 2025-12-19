using Application.DTOs.Comment;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Application.Services;

/// <summary>
///     Xử lý logic liên quan đến bình luận
/// </summary>
public class CommentService : ICommentService
{
    private readonly IMapper _mapper;
    private readonly ICommentRepository _repository;
    private readonly ICacheService _cache;
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public CommentService(ICommentRepository repository, IMapper mapper, ICacheService cache)
    {
        _repository = repository;
        _mapper = mapper;
        _cache = cache;
    }

    /// <summary>
    ///     Lấy cây bình luận cho một bài viết
    /// </summary>
    public async Task<List<CommentDto>> GetCommentTreeAsync(Guid postId)
    {
        var cacheKey = GetTreeCacheKey(postId);

        var cachedTree = await _cache.GetAsync<List<CommentDto>>(cacheKey);
        if (cachedTree != null)
            return cachedTree;

        var comments = await _repository.GetAllCommentsForPost(postId);
        var tree = MapToCommentTree(comments);

        await _cache.SetAsync(cacheKey, tree, CacheTtl);
        return tree;
    }

    /// <summary>
    ///     Lấy danh sách bình luận phẳng cho một bài viết
    /// </summary>
    public async Task<List<CommentFlattenDto>> GetCommentFlattenAsync(Guid postId)
    {
        var cacheKey = GetFlattenCacheKey(postId);

        var cachedFlatten = await _cache.GetAsync<List<CommentFlattenDto>>(cacheKey);
        if (cachedFlatten != null)
            return cachedFlatten;

        var comments = await _repository.GetAllCommentsRecursive(postId);
        var flatten = FlattenComments(comments);

        await _cache.SetAsync(cacheKey, flatten, CacheTtl);
        return flatten;
    }

    /// <summary>
    ///     Tạo mới bình luận
    /// </summary>
    public async Task<CommentDto> CreateCommentAsync(CommentCreateDto dto)
    {
        var comment = new Comment
        {
            Content = dto.Content,
            UserId = dto.UserId,
            PostId = dto.PostId,
            ParentCommentId = dto.ParentCommentId
        };

        var created = await _repository.CreateAsync(comment);

        await InvalidateCacheAsync(dto.PostId);
        return MapCommentRecursive(created, 0);
    }

    /// <summary>
    ///     Cập nhật bình luận
    /// </summary>
    public async Task<CommentDto?> UpdateCommentAsync(Guid id, CommentUpdateDto dto)
    {
        var updated = await _repository.UpdateAsync(id, dto.Content);
        if (updated == null)
            return null;

        await InvalidateCacheAsync(updated.PostId);
        return MapCommentRecursive(updated, 0);
    }

    /// <summary>
    ///     Xóa bình luận
    /// </summary>
    public async Task<bool> DeleteCommentAsync(Guid id)
    {
        var comment = await _repository.GetByIdAsync(id);
        if (comment == null)
            return false;

        var result = await _repository.DeleteAsync(id);
        if (result)
            await InvalidateCacheAsync(comment.PostId);

        return result;
    }
    
    /// <summary>
    /// Cache helpers
    /// </summary>
    private static string GetTreeCacheKey(Guid postId)
        => $"CommentTree:{postId}";

    private static string GetFlattenCacheKey(Guid postId)
        => $"CommentFlatten:{postId}";

    private async Task InvalidateCacheAsync(Guid postId)
    {
        await _cache.RemoveAsync(GetTreeCacheKey(postId));
        await _cache.RemoveAsync(GetFlattenCacheKey(postId));
    }

    /// <summary>
    ///     Map danh sách bình luận thành cây bình luận
    /// </summary>
    private List<CommentDto> MapToCommentTree(List<Comment> comments)
    {
        var result = new List<CommentDto>();

        foreach (var comment in comments) result.Add(MapCommentRecursive(comment, 0));

        return result;
    }
    
    /// <summary>
    ///     Map bình luận và các phản hồi đệ quy
    /// </summary>
    private CommentDto MapCommentRecursive(Comment comment, int depth)
    {
        var dto = _mapper.Map<CommentDto>(comment);
        dto.Depth = depth;

        if (comment.Replies.Any())
        {
            dto.Replies = comment.Replies
                .Select(reply => MapCommentRecursive(reply, depth + 1))
                .ToList();
        }

        return dto;
    }

    /// <summary>
    ///     Chuyển danh sách bình luận thành danh sách phẳng có thông tin depth và path
    /// </summary>
    private List<CommentFlattenDto> FlattenComments(List<Comment> comments)
    {
        var result = new List<CommentFlattenDto>();
        var commentDict = comments.ToDictionary(c => c.Id);
        var roots = comments.Where(c => c.ParentCommentId == null).ToList();

        foreach (var root in roots)
            FlattenRecursive(root, commentDict, result, 0, "");

        return result;
    }

    /// <summary>
    ///     Đệ quy flatten bình luận và các phản hồi
    /// </summary>
    private void FlattenRecursive(
        Comment comment,
        Dictionary<Guid, Comment> allComments,
        List<CommentFlattenDto> result,
        int depth,
        string path)
    {
        var currentPath = string.IsNullOrEmpty(path)
            ? "1"
            : $"{path}.{result.Count(c => c.Path.StartsWith(path)) + 1}";

        var dto = _mapper.Map<CommentFlattenDto>(comment);
        dto.Depth = depth;
        dto.Path = currentPath;
        result.Add(dto);

        var replies = allComments.Values
            .Where(c => c.ParentCommentId == comment.Id)
            .ToList();

        foreach (var reply in replies)
            FlattenRecursive(reply, allComments, result, depth + 1, currentPath);
    }
}
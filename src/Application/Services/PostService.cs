using Application.DTOs.Post;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;

namespace Application.Services;

public class PostService : IPostService
{
    private readonly IPostRepository _repository;
    private readonly IMapper _mapper;
    private readonly ICacheService _cache;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(5);

    public PostService(IPostRepository repository, IMapper mapper, ICacheService cache)
    {
        _repository = repository;
        _mapper = mapper;
        _cache = cache;
    }

    public async Task<PostDto?> GetByIdAsync(Guid id)
    {
        var cacheKey = GetPostCacheKey(id);

        var cached = await _cache.GetAsync<PostDto>(cacheKey);
        if (cached != null)
            return cached;

        var post = await _repository.GetByIdAsync(id);
        if (post == null)
            return null;

        var dto = _mapper.Map<PostDto>(post);
        await _cache.SetAsync(cacheKey, dto, CacheTtl);
        return dto;
    }

    public async Task<PagedResultDto<PostDto>> GetPagedAsync(PostQueryParametersDto parameters)
    {
        var pagedPosts = await _repository.GetPagedAsync(parameters);
        var dtoItems = _mapper.Map<List<PostDto>>(pagedPosts.Items);

        return new PagedResultDto<PostDto>
        {
            Items = dtoItems,
            TotalCount = pagedPosts.TotalCount,
            PageNumber = pagedPosts.PageNumber,
            PageSize = pagedPosts.PageSize
        };
    }

    public async Task<PostDto> CreatePostAsync(PostCreateDto dto)
    {
        var entity = _mapper.Map<Post>(dto);
        var created = await _repository.CreateAsync(entity);
        return _mapper.Map<PostDto>(created);
    }

    public async Task<PostDto?> UpdatePostAsync(Guid postId, Guid userId, bool isAdmin, PostUpdateDto dto)
    {
        var post = await _repository.GetByIdAsync(postId);
        if (post == null)
            return null;

        if (!isAdmin && post.AuthorId != userId)
            throw new UnauthorizedAccessException("u ain't allowed to update this post");

        if (dto.Title != null)
            post.Title = dto.Title;

        if (dto.Content != null)
            post.Content = dto.Content;

        await _repository.UpdateAsync(post);
        await _cache.RemoveAsync(GetPostCacheKey(postId));

        return _mapper.Map<PostDto>(post);
    }

    public async Task<bool> DeletePostAsync(Guid postId, Guid userId, bool isAdmin)
    {
        var post = await _repository.GetByIdAsync(postId);
        if (post == null)
            return false;

        if (!isAdmin && post.AuthorId != userId)
            throw new UnauthorizedAccessException("u ain't allowed to delete this post");

        var result = await _repository.DeleteAsync(postId);
        await _cache.RemoveAsync(GetPostCacheKey(postId));
        return result;
    }

    private static string GetPostCacheKey(Guid id) => $"Post:{id}";
}

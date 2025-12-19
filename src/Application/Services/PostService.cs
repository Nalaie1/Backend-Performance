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

    public PostService(
        IPostRepository repository,
        IMapper mapper,
        ICacheService cache)
    {
        _repository = repository;
        _mapper = mapper;
        _cache = cache;
    }

    /// <summary>
    ///     Lấy bài viết theo Id (có cache)
    /// </summary>
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

    /// <summary>
    ///     Lấy danh sách bài viết có phân trang, lọc, sắp xếp
    /// </summary>
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

    /// <summary>
    ///     Tạo mới bài viết
    /// </summary>
    public async Task<PostDto> CreatePostAsync(PostCreateDto dto)
    {
        var entity = _mapper.Map<Post>(dto);
        var created = await _repository.CreateAsync(entity);

        // Có thể invalidate list cache sau này
        return _mapper.Map<PostDto>(created);
    }

    /// <summary>
    ///     Cập nhật bài viết
    /// </summary>
    public async Task<PostDto?> UpdatePostAsync(Guid id, PostUpdateDto dto)
    {
        var updatedEntity = await _repository.UpdateAsync(id, dto.Content);
        if (updatedEntity == null)
            return null;

        await _cache.RemoveAsync(GetPostCacheKey(id));
        return _mapper.Map<PostDto>(updatedEntity);
    }

    /// <summary>
    ///     Xóa bài viết
    /// </summary>
    public async Task<bool> DeletePostAsync(Guid id)
    {
        var result = await _repository.DeleteAsync(id);

        if (result)
            await _cache.RemoveAsync(GetPostCacheKey(id));

        return result;
    }

    // =========================
    // Cache helpers
    // =========================

    private static string GetPostCacheKey(Guid id)
        => $"Post:{id}";
}

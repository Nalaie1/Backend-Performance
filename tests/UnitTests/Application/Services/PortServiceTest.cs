using Application.DTOs.Post;
using Application.Interfaces;
using Application.Services;
using AutoMapper;
using Domain.Entities;
using Moq;
using Xunit;

namespace UnitTests.Application.Services;

public class PostServiceTests
{
    private readonly Mock<IPostRepository> _repoMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly PostService _service;

    public PostServiceTests()
    {
        _repoMock = new Mock<IPostRepository>();
        _mapperMock = new Mock<IMapper>();
        _cacheMock = new Mock<ICacheService>();

        _service = new PostService(
            _repoMock.Object,
            _mapperMock.Object,
            _cacheMock.Object
        );
    }

    // GetByIdAsync
    [Fact]
    public async Task GetByIdAsync_ShouldReturnCachedPost_WhenCacheExists()
    {
        var postId = Guid.NewGuid();
        var cachedDto = new PostDto { Id = postId };

        _cacheMock
            .Setup(c => c.GetAsync<PostDto>($"Post:{postId}"))
            .ReturnsAsync(cachedDto);

        var result = await _service.GetByIdAsync(postId);

        Assert.NotNull(result);
        Assert.Equal(postId, result!.Id);

        _repoMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldGetFromRepoAndSetCache_WhenCacheMiss()
    {
        var postId = Guid.NewGuid();
        var post = new Post { Id = postId };
        var dto = new PostDto { Id = postId };

        _cacheMock
            .Setup(c => c.GetAsync<PostDto>($"Post:{postId}"))
            .ReturnsAsync((PostDto?)null);

        _repoMock
            .Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync(post);

        _mapperMock
            .Setup(m => m.Map<PostDto>(post))
            .Returns(dto);

        var result = await _service.GetByIdAsync(postId);

        Assert.NotNull(result);
        Assert.Equal(postId, result!.Id);

        _cacheMock.Verify(
            c => c.SetAsync($"Post:{postId}", dto, It.IsAny<TimeSpan>()),
            Times.Once
        );
    }
    
    /// CreatePostAsync
    [Fact]
    public async Task CreatePostAsync_ShouldCreatePostAndReturnDto()
    {
        var createDto = new PostCreateDto { Title = "t", Content = "c" };
        var entity = new Post { Id = Guid.NewGuid(), Title = "t", Content = "c" };
        var dto = new PostDto { Id = entity.Id, Title = "t", Content = "c" };

        _mapperMock.Setup(m => m.Map<Post>(createDto)).Returns(entity);
        _repoMock.Setup(r => r.CreateAsync(entity)).ReturnsAsync(entity);
        _mapperMock.Setup(m => m.Map<PostDto>(entity)).Returns(dto);

        var result = await _service.CreatePostAsync(createDto);

        Assert.Equal(dto.Id, result.Id);
        _repoMock.Verify(r => r.CreateAsync(entity), Times.Once);
    }

    // UpdatePostAsync
    [Fact]
    public async Task UpdatePostAsync_ShouldReturnNull_WhenPostNotFound()
    {
        var postId = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync((Post?)null);

        var result = await _service.UpdatePostAsync(
            postId,
            Guid.NewGuid(),
            false,
            new PostUpdateDto()
        );

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdatePostAsync_ShouldThrow_WhenNotAuthorAndNotAdmin()
    {
        var postId = Guid.NewGuid();
        var post = new Post
        {
            Id = postId,
            AuthorId = Guid.NewGuid()
        };

        _repoMock.Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync(post);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.UpdatePostAsync(
                postId,
                Guid.NewGuid(),
                false,
                new PostUpdateDto()
            )
        );
    }

    [Fact]
    public async Task UpdatePostAsync_ShouldUpdateAndInvalidateCache_WhenAuthorized()
    {
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var post = new Post
        {
            Id = postId,
            AuthorId = userId,
            Title = "old"
        };

        var updateDto = new PostUpdateDto { Title = "new" };
        var dto = new PostDto { Id = postId, Title = "new" };

        _repoMock.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync(post);
        _mapperMock.Setup(m => m.Map<PostDto>(post)).Returns(dto);

        var result = await _service.UpdatePostAsync(
            postId,
            userId,
            false,
            updateDto
        );

        Assert.Equal("new", result!.Title);

        _repoMock.Verify(r => r.UpdateAsync(post), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync($"Post:{postId}"), Times.Once);
    }

    // DeletePostAsync
    [Fact]
    public async Task DeletePostAsync_ShouldReturnFalse_WhenPostNotFound()
    {
        var postId = Guid.NewGuid();

        _repoMock.Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync((Post?)null);

        var result = await _service.DeletePostAsync(postId, Guid.NewGuid(), false);

        Assert.False(result);
    }

    [Fact]
    public async Task DeletePostAsync_ShouldThrow_WhenNotAuthorAndNotAdmin()
    {
        var postId = Guid.NewGuid();

        var post = new Post
        {
            Id = postId,
            AuthorId = Guid.NewGuid()
        };

        _repoMock.Setup(r => r.GetByIdAsync(postId))
            .ReturnsAsync(post);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.DeletePostAsync(postId, Guid.NewGuid(), false)
        );
    }

    [Fact]
    public async Task DeletePostAsync_ShouldDeleteAndInvalidateCache_WhenAuthorized()
    {
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var post = new Post
        {
            Id = postId,
            AuthorId = userId
        };

        _repoMock.Setup(r => r.GetByIdAsync(postId)).ReturnsAsync(post);
        _repoMock.Setup(r => r.DeleteAsync(postId)).ReturnsAsync(true);

        var result = await _service.DeletePostAsync(postId, userId, false);

        Assert.True(result);
        _cacheMock.Verify(c => c.RemoveAsync($"Post:{postId}"), Times.Once);
    }
}

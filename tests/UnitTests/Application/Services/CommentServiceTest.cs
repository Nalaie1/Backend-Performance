using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.DTOs.Comment;
using Application.Services;
using Application.Interfaces;
using AutoMapper;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace UnitTests.Application.Services
{
    public class CommentServiceTest
    {
        private readonly Mock<ICommentRepository> _repoMock = new();
        private readonly Mock<IMapper> _mapperMock = new();
        private readonly Mock<ICacheService> _cacheMock = new();
        private readonly CommentService _service;

        public CommentServiceTest()
        {
            _service = new CommentService(_repoMock.Object, _mapperMock.Object, _cacheMock.Object);

            // Default mapper behavior: map domain Comment -> DTOs copying fields used in tests
            _mapperMock.Setup(m => m.Map<CommentDto>(It.IsAny<Comment>()))
                .Returns((Comment c) => new CommentDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    UserId = c.UserId,
                    PostId = c.PostId,
                    ParentCommentId = c.ParentCommentId,
                    Replies = new List<CommentDto>()
                });

            _mapperMock.Setup(m => m.Map<CommentFlattenDto>(It.IsAny<Comment>()))
                .Returns((Comment c) => new CommentFlattenDto
                {
                    Id = c.Id,
                    Content = c.Content,
                    UserId = c.UserId,
                    PostId = c.PostId,
                    ParentCommentId = c.ParentCommentId
                });
        }

        private static Comment CreateComment(Guid id, Guid postId, Guid? parentId = null, string content = "c")
        {
            return new Comment
            {
                Id = id,
                Content = content,
                UserId = Guid.NewGuid(),
                PostId = postId,
                ParentCommentId = parentId,
                Replies = new List<Comment>()
            };
        }

        [Fact]
        public async Task GetCommentTreeAsync_ReturnsCached_WhenCacheHit()
        {
            var postId = Guid.NewGuid();
            var cacheKey = $"CommentTree:{postId}";

            var cached = new List<CommentDto> { new CommentDto { Id = Guid.NewGuid(), Content = "cached" } };
            _cacheMock.Setup(c => c.GetAsync<List<CommentDto>>(cacheKey)).ReturnsAsync(cached);

            var result = await _service.GetCommentTreeAsync(postId);

            result.Should().BeEquivalentTo(cached);
            _repoMock.Verify(r => r.GetAllCommentsForPost(It.IsAny<Guid>(), It.IsAny<bool>()), Times.Never);
            _cacheMock.Verify(c => c.GetAsync<List<CommentDto>>(cacheKey), Times.Once);
        }

        [Fact]
        public async Task GetCommentTreeAsync_ReturnsTreeAndSetsCache_WhenCacheMiss()
        {
            var postId = Guid.NewGuid();
            var cacheKey = $"CommentTree:{postId}";

            _cacheMock.Setup(c => c.GetAsync<List<CommentDto>>(cacheKey)).ReturnsAsync((List<CommentDto>?)null);

            // build nested comments: root -> child -> grandchild
            var root = CreateComment(Guid.NewGuid(), postId);
            var child = CreateComment(Guid.NewGuid(), postId, root.Id);
            var grand = CreateComment(Guid.NewGuid(), postId, child.Id);

            // wire Replies for mapping recursion
            root.Replies.Add(child);
            child.Replies.Add(grand);

            _repoMock.Setup(r => r.GetAllCommentsForPost(postId, It.IsAny<bool>())).ReturnsAsync(new List<Comment> { root });

            var result = await _service.GetCommentTreeAsync(postId);

            result.Should().HaveCount(1);
            var rDto = result[0];
            rDto.Id.Should().Be(root.Id);
            rDto.Depth.Should().Be(0);
            rDto.Replies.Should().HaveCount(1);
            rDto.Replies[0].Id.Should().Be(child.Id);
            rDto.Replies[0].Depth.Should().Be(1);
            rDto.Replies[0].Replies[0].Id.Should().Be(grand.Id);
            rDto.Replies[0].Replies[0].Depth.Should().Be(2);

            _cacheMock.Verify(c => c.SetAsync(cacheKey, It.IsAny<List<CommentDto>>(), It.IsAny<TimeSpan>()), Times.Once);
            _repoMock.Verify(r => r.GetAllCommentsForPost(postId, It.IsAny<bool>()), Times.Once);
        }

        [Fact]
        public async Task GetCommentFlattenAsync_ReturnsCached_WhenCacheHit()
        {
            var postId = Guid.NewGuid();
            var cacheKey = $"CommentFlatten:{postId}";

            var cached = new List<CommentFlattenDto> { new CommentFlattenDto { Id = Guid.NewGuid(), Content = "cached" } };
            _cacheMock.Setup(c => c.GetAsync<List<CommentFlattenDto>>(cacheKey)).ReturnsAsync(cached);

            var result = await _service.GetCommentFlattenAsync(postId);

            result.Should().BeEquivalentTo(cached);
            _repoMock.Verify(r => r.GetAllCommentsRecursive(It.IsAny<Guid>()), Times.Never);
            _cacheMock.Verify(c => c.GetAsync<List<CommentFlattenDto>>(cacheKey), Times.Once);
        }

        [Fact]
        public async Task GetCommentFlattenAsync_FlattensNestedCommentsCorrectly_WhenCacheMiss()
        {
            var postId = Guid.NewGuid();
            var cacheKey = $"CommentFlatten:{postId}";
            _cacheMock.Setup(c => c.GetAsync<List<CommentFlattenDto>>(cacheKey)).ReturnsAsync((List<CommentFlattenDto>?)null);

            // build comments: root1 -> r1.1 -> r1.1.1 ; root2 -> r2.1
            var root1 = CreateComment(Guid.NewGuid(), postId);
            var r11 = CreateComment(Guid.NewGuid(), postId, root1.Id);
            var r111 = CreateComment(Guid.NewGuid(), postId, r11.Id);

            var root2 = CreateComment(Guid.NewGuid(), postId);
            var r21 = CreateComment(Guid.NewGuid(), postId, root2.Id);

            // For flatten, service expects GetAllCommentsRecursive to return a list of all comments (not necessarily nested Replies)
            var all = new List<Comment> { root1, r11, r111, root2, r21 };
            _repoMock.Setup(r => r.GetAllCommentsRecursive(postId)).ReturnsAsync(all);

            var result = await _service.GetCommentFlattenAsync(postId);

            // should contain 5 items
            result.Should().HaveCount(5);

            // find root1 and verify its path and depth
            var fRoot1 = result.First(r => r.Id == root1.Id);
            fRoot1.Depth.Should().Be(0);
            fRoot1.Path.Should().StartWith("1");

            var fR11 = result.First(r => r.Id == r11.Id);
            fR11.Depth.Should().Be(1);
            fR11.Path.Should().StartWith(fRoot1.Path + ".");

            var fR111 = result.First(r => r.Id == r111.Id);
            fR111.Depth.Should().Be(2);
            fR111.Path.Should().StartWith(fR11.Path + ".");

            var fRoot2 = result.First(r => r.Id == root2.Id);
            fRoot2.Depth.Should().Be(0);
            // root2 path should not equal root1 path
            fRoot2.Path.Should().NotBe(fRoot1.Path);

            _cacheMock.Verify(c => c.SetAsync(cacheKey, It.IsAny<List<CommentFlattenDto>>(), It.IsAny<TimeSpan>()), Times.Once);
            _repoMock.Verify(r => r.GetAllCommentsRecursive(postId), Times.Once);
        }
    }
}

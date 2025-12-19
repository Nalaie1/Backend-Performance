using System.Linq.Dynamic.Core;
using Application.DTOs.Post;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly AppDbContext _context;

    public PostRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDto<Post>> GetPagedAsync(PostQueryParametersDto parameters)
    {
        var query = _context.Posts
            .Include(p => p.User)
            .Include(p => p.Comments)
            .AsQueryable();

        // Filtering
        if (!string.IsNullOrEmpty(parameters.SearchTerm))
            query = query.Where(p =>
                p.Title.Contains(parameters.SearchTerm) ||
                p.Content.Contains(parameters.SearchTerm));

        if (parameters.UserId.HasValue)
            query = query.Where(p => p.AuthorId == parameters.UserId.Value);

        // Sorting với Dynamic LINQ
        if (!string.IsNullOrEmpty(parameters.SortBy))
        {
            var sortDirection = parameters.SortDirection?.ToLower() == "desc" ? "descending" : "ascending";
            query = query.OrderBy($"{parameters.SortBy} {sortDirection}");
        }
        else
        {
            query = query.OrderByDescending(p => p.Id);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResultDto<Post>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    public async Task<Post?> GetByIdAsync(Guid postId)
    {
        return await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == postId);
    }

    public async Task<Post> CreateAsync(Post post)
    {
        post.Id = Guid.NewGuid();
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task<Post?> UpdateAsync(Post post)
    {
        _context.Posts.Update(post);
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task<bool> DeleteAsync(Guid postId)
    {
        var post = await _context.Posts.FindAsync(postId);
        if (post == null)
            return false;

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
        return true;
    }
}

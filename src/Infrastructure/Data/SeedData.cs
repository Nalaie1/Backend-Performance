using Bogus;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.SeedData;

public static class SeedData
{
    // Phương thức để tạo dữ liệu mẫu sử dụng Bogus
    public static async Task SeedAsync(AppDbContext context)
    {
        var rand = new Random();
        const int batchSize = 5_000;
        const int totalUsers = 100_000;
        const int totalPosts = 200_000;
        const int totalComments = 500_000;

        await context.Database.MigrateAsync();

        context.ChangeTracker.AutoDetectChangesEnabled = false;

        await SeedUsers(context, totalUsers, batchSize);
        await SeedPosts(context, totalPosts, batchSize, rand);
        await SeedComments(context, totalComments, batchSize, rand);
        await SeedReplies(context, rand);

        context.ChangeTracker.AutoDetectChangesEnabled = true;
    }

    private static async Task SeedUsers(AppDbContext context, int total, int batchSize)
    {
        if (context.Users.Any()) return;

        Console.WriteLine("Seeding Users...");
        for (int i = 0; i < total; i += batchSize)
        {
            var users = Enumerable.Range(0, Math.Min(batchSize, total - i))
                .Select(j => new User
                {
                    Id = Guid.NewGuid(),
                    Name = $"User {i + j + 1}"
                })
                .ToList();

            context.Users.AddRange(users);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            Console.WriteLine($"Inserted Users {i + 1} - {i + users.Count}");
        }
    }

    private static async Task SeedPosts(AppDbContext context, int total, int batchSize, Random rand)
    {
        if (context.Posts.Any()) return;

        Console.WriteLine("Seeding Posts...");
        var userIds = context.Users.Select(u => u.Id).ToList();

        for (int i = 0; i < total; i += batchSize)
        {
            var posts = Enumerable.Range(0, Math.Min(batchSize, total - i))
                .Select(j => new Post
                {
                    Id = Guid.NewGuid(),
                    Title = $"Post {i + j + 1}",
                    Content = $"Content {i + j + 1}",
                    UserId = userIds[rand.Next(userIds.Count)]
                })
                .ToList();

            context.Posts.AddRange(posts);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            Console.WriteLine($"Inserted Posts {i + 1} - {i + posts.Count}");
        }
    }

    private static async Task SeedComments(AppDbContext context, int total, int batchSize, Random rand)
    {
        if (context.Comments.Any(c => c.ParentCommentId == null)) return;

        Console.WriteLine("Seeding Comments...");
        var userIds = context.Users.Select(u => u.Id).ToList();
        var postIds = context.Posts.Select(p => p.Id).ToList();

        for (int i = 0; i < total; i += batchSize)
        {
            var comments = Enumerable.Range(0, Math.Min(batchSize, total - i))
                .Select(j => new Comment
                {
                    Id = Guid.NewGuid(),
                    Content = $"Comment {i + j + 1}",
                    UserId = userIds[rand.Next(userIds.Count)],
                    PostId = postIds[rand.Next(postIds.Count)],
                    ParentCommentId = null
                })
                .ToList();

            context.Comments.AddRange(comments);
            await context.SaveChangesAsync();
            context.ChangeTracker.Clear();

            Console.WriteLine($"Inserted Comments {i + 1} - {i + comments.Count}");
        }
    }

    private static async Task SeedReplies(AppDbContext context, Random rand)
    {
        if (context.Comments.Any(c => c.ParentCommentId != null)) return;

        Console.WriteLine("Seeding Replies...");
        var userIds = context.Users.Select(u => u.Id).ToList();
        var parents = context.Comments
            .Where(c => c.ParentCommentId == null)
            .AsNoTracking()
            .ToList();

        var replies = new List<Comment>();

        foreach (var parent in parents)
        {
            var replyCount = rand.Next(0, 4);
            for (int i = 0; i < replyCount; i++)
            {
                replies.Add(new Comment
                {
                    Id = Guid.NewGuid(),
                    Content = $"Reply to {parent.Id}",
                    UserId = userIds[rand.Next(userIds.Count)],
                    PostId = parent.PostId,
                    ParentCommentId = parent.Id
                });
            }

            if (replies.Count >= 5_000)
            {
                context.Comments.AddRange(replies);
                await context.SaveChangesAsync();
                context.ChangeTracker.Clear();
                replies.Clear();
            }
        }

        if (replies.Any())
        {
            context.Comments.AddRange(replies);
            await context.SaveChangesAsync();
        }

        Console.WriteLine("Replies seeded!");
    }
}

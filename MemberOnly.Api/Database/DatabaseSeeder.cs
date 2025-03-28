using MemberOnly.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MemberOnly.Api.Services.Infrastructure;


namespace MemberOnly.Api.Database;

public class DatabaseSeeder
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;
    private readonly PasswordHasher _passwordHasher;

    public DatabaseSeeder(ApplicationDbContext context, ILogger<DatabaseSeeder> logger , PasswordHasher passwordHasher)
    {
        _context = context;
        _logger = logger;
        _passwordHasher = passwordHasher;
    }


    public async Task SeedAsync()
    {
        try 
        {
            
            await _context.Database.MigrateAsync();
            
            _logger.LogInformation("Clearing database tables...");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"RefreshTokens\", \"Posts\" RESTART IDENTITY CASCADE;");
            await _context.Database.ExecuteSqlRawAsync("TRUNCATE TABLE \"Users\" RESTART IDENTITY CASCADE;");
            _logger.LogInformation("Database tables cleared successfully");
            
            await SeedUsersAsync();
            await SeedPostsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while resetting database");
            throw;
        }
    }

    private async Task SeedUsersAsync()
    {
        if (await _context.Users.AnyAsync())
        {
            _logger.LogInformation("Users table already seeded");
            return;
        }
        
        _logger.LogInformation("Seeding users table");
        
        var users = new List<User>
        {
            new User
            {
                Username = "john_doe",
                FirstName = "john_doe",
                LastName = "john_doe",
                PasswordHash = _passwordHasher.Hash("password123")
            },
            new User
            {
                Username = "jane_smith",
                FirstName = "jane_smith",
                LastName = "jane_smith",
                PasswordHash = _passwordHasher.Hash("password456")
            },
            new User
            {
                Username = "admin",
                FirstName = "admin",
                LastName = "admin",
                PasswordHash =  _passwordHasher.Hash("admin123")
            }
        };
        
        await _context.Users.AddRangeAsync(users);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Added {Count} sample users", users.Count);
    }

    private async Task SeedPostsAsync()
    {
        if (await _context.Posts.AnyAsync())
        {
            _logger.LogInformation("Posts table already seeded");
            return;
        }

        if (!await _context.Users.AnyAsync())
        {
            _logger.LogWarning("Cannot seed posts: No users exist in the database");
            return;
        }
        
        _logger.LogInformation("Seeding posts table");
        
        var users = await _context.Users.ToListAsync();
        var random = new Random();
        
        var posts = new List<Post>();
        
        foreach (var user in users)
        {
            var postCount = random.Next(2, 6);
            for (int i = 0; i < postCount; i++)
            {
                posts.Add(new Post
                {
                    Title = $"Post {i + 1} by {user.Username}",
                    Content = $"This is sample content for post {i + 1} created by {user.Username}. Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
                    Username = user.Username,
                    CreatedAt = DateTime.UtcNow.AddDays(-random.Next(0, 30))
                });
            }
        }
        
        await _context.Posts.AddRangeAsync(posts);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Added {Count} sample posts", posts.Count);
    }
}

using MemberOnly.Api.Database;
using MemberOnly.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MemberOnly.Api.Services;

public class PostService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PostService> _logger;
    
    public PostService(ApplicationDbContext context , ILogger<PostService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Post>> GetPostsAsyncByUsername(string username)
    {

        return await _context.Posts
            .Where(x => x.Username == username )
            .ToListAsync();
    }
    
    public async Task<Post> CreatePostAsync(Post post)
    {
        if (post == null)
            throw new ArgumentNullException(nameof(post));
            
        if (!_context.Users.Any(u => u.Username == post.Username))
        {
            _logger.LogWarning("Creation failed: Username {Username} does not exist", post.Username);
            throw new Exception("Username does not exist");
        }
        
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }
    
    public async Task<Post> GetPostByIdAsync(int id)
    {
        return await _context.Posts
            .FirstOrDefaultAsync(x => x.Id == id);
    }
    

    
    public async Task DeletePostAsync(int id)
    {
        var post = await GetPostByIdAsync(id);
        if (post == null)
        {
            return;
        }
        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
    }
    
    
    
    public async Task<List<Post>> GetAllPostsAsync()
    {
        return await _context.Posts
            .ToListAsync();
    }

}
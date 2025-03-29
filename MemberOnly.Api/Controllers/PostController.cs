using MemberOnly.Api.Models;
using MemberOnly.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MemberOnly.Api.Controllers;

using JWTClaims = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

[ApiController]
[Route("api/[controller]")]
public class PostController : ControllerBase
{
    private readonly ILogger<PostController> _logger;
    private readonly PostService _postService;
    
    
    public PostController(ILogger<PostController> logger, PostService postService)
    {
        _logger = logger;
        _postService = postService;
    }
    
    [HttpGet("user/{username}")]
    [Authorize]
    public async Task<IActionResult> GetPostsByUserName(string username)
    {
        var currentUsernameClaim = User.FindFirst(JWTClaims.Name);
        if (currentUsernameClaim == null)
        {
            return Forbid();
        }

        if (!User.IsInRole("Admin") && currentUsernameClaim.Value != username)
        {
            return Forbid();
        }

        var posts = await _postService.GetPostsAsyncByUsername(username);
        return Ok(posts);
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetPostById(int id)
    {
        if ( id <= 0)
        {
            return BadRequest("Invalid post ID");
        }
        var currentUsernameClaim = User.FindFirst(JWTClaims.Name);
        if (currentUsernameClaim == null)
        {
            return Forbid();
        }
    
        var post = await _postService.GetPostByIdAsync(id);
        if (post == null) return NotFound();
        if ( post.Username != currentUsernameClaim.Value && !User.IsInRole("Admin"))
        {
            return Forbid();
        }
        return Ok(post);
    }
    
    [HttpGet("myPosts")]
    [Authorize]
    public async Task<IActionResult> GetMyPosts()
    {
        var currentUsernameClaim = User.FindFirst(JWTClaims.Name);
        if (currentUsernameClaim == null)
        {
            return Forbid();
        }
        return await GetPostsByUserName(currentUsernameClaim.Value);
    }
    
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreatePost([FromBody] Post post)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
            
        var currentUsernameClaim = User.FindFirst(JWTClaims.Name);
        if (currentUsernameClaim == null)
            return Forbid();
            
        post.Username = currentUsernameClaim.Value;
        var createdPost = await _postService.CreatePostAsync(post);
        return CreatedAtAction(nameof(GetPostById), new { id = createdPost.Id }, createdPost);
    }
    
  
    
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeletePost(int id)
    {
        if( id <= 0)
            return BadRequest("Invalid post ID");

        var currentUsernameClaim = User.FindFirst(JWTClaims.Name);
        if (currentUsernameClaim == null)
            return Forbid();
            
        var existingPost = await _postService.GetPostByIdAsync(id);
        if (existingPost == null)
            return NotFound();
            
        if (!User.IsInRole("Admin") && existingPost.Username != currentUsernameClaim.Value)
            return Forbid();
            
        await _postService.DeletePostAsync(id);
        return NoContent();
    }
    
    
    [HttpGet("all")]
    [Authorize]
    public async Task<IActionResult> GetAllPosts(){
        var posts = await _postService.GetAllPostsAsync();
        return Ok(posts);
    }
    
    [HttpPost]
    [Route("create")]
    [Authorize]
    public async Task<IActionResult> CreatePost([FromBody] PostAdd addPost)
    {
        _logger.LogInformation("Creating new post" , addPost);
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        var currentUsernameClaim = User.FindFirst(JWTClaims.Name);
        if (currentUsernameClaim == null)
            return Forbid();
            
        var post = await _postService.CreatePostAsync(currentUsernameClaim.Value, addPost.Content, addPost.Title);
        return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, post);
    }

}
using System;
using MemberOnly.Api.Models;
using MemberOnly.Api.Services;
using MemberOnly.Api.Services.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MemberOnly.Api.Controllers;
using JWTClaims = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;
    private readonly RefreshTokenService _refreshTokenService;
    private readonly TokenProvider _tokenProvider;
    private readonly ILogger<UserController> _logger;

    public UserController(UserService userService, RefreshTokenService refreshTokenService,
        ILogger<UserController> logger , TokenProvider tokenProvider)
    {
        _userService = userService;
        _refreshTokenService = refreshTokenService;
        _logger = logger;
        _tokenProvider = tokenProvider;
    }
    [HttpGet("check-username")]
    
    public async Task<IActionResult> UserNameCheck([FromQuery]string username)
    {
        if( username == null)
        {
            return BadRequest();
        }
        var posts = await _userService.UserNameCheckAsync(username);
        return Ok(posts);
    }

    [HttpGet("myposts")]
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

    [HttpGet("{username}/posts")]
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

        var posts = await _userService.GetPostsAsyncByUsername(username);
        return Ok(posts);
    }

    [HttpGet("{username}/info")]
    [Authorize]
    public async Task<IActionResult> GetUserInfo(string username)
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
        
        var info = await _userService.GetUserInfoAsync(username);   
        return Ok(info);
        
    }
    

    [HttpPost("register")]
    public async Task<IActionResult> RegisterUser(UserRegister UserReg)
    {
        try
        {
            var registeredUser = await _userService.RegisterUserAsync(UserReg);
            return Ok(registeredUser);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid registration attempt: {Message}", ex.Message);
            return BadRequest(new { message = "Registration failed. Username may already exist." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user");
            return StatusCode(500, new { message = "An unexpected error occurred during registration." });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(UserLogin user)
    {
        try
        {
            var loggedInUser = await _userService.LoginUserAsync(user);
            return Ok(loggedInUser);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Login failed: {Message}", ex.Message);
            return Unauthorized(new { message = "Invalid username or password." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging in user");
            return StatusCode(500, new { message = "An unexpected error occurred during login." });
        }
    }

    [HttpPost("refresh-token")]
    [Authorize]
    public async Task<IActionResult> RefreshToken(RefreshTokenFront refreshTokenRequest)
    {
        try
        {
            var refreshToken = await _refreshTokenService.ValidateAndRotateTokenAsync(refreshTokenRequest);
            return Ok(refreshToken);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid refresh token: {Message}", ex.Message);
            return Unauthorized(new { message = "Invalid or expired refresh token." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing token");
            return StatusCode(500, new { message = "An unexpected error occurred while refreshing the token." });
        }
    }


    [HttpGet("validate")]
    public IActionResult ValidateJWT()
    {
        // Extract token from Authorization header
        var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
    
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return Unauthorized(new { valid = false, message = "Authorization header missing or invalid." });
    
        // Remove "Bearer " prefix
        var token = authHeader.Substring(7);
    
        try
        {
            var principal = _tokenProvider.validate(token);
            if (principal == null)
                return Unauthorized(new { valid = false, message = "Invalid token." });
            
            //var claims = principal.Claims.Select(c => new { c.Type, c.Value });
            var username = principal.FindFirst(JWTClaims.Name)?.Value;
            //return Ok(new { valid = true, claims });
            
            return Ok(new { valid = true, username });
        }
        catch (Exception)
        {
            return Unauthorized(new { valid = false, message = "Invalid token." });
        }
    }
    

    


    [HttpPost("logout")]
    public async Task<IActionResult> Logout(RefreshTokenFront refreshTokenRequest)
    {
        try
        {
            _logger.LogInformation("Logging out user");
            var result = await _refreshTokenService.DeleteRefreshTokenAsync(refreshTokenRequest);
            
            if (!result)
            {
                _logger.LogWarning("Token not found during logout");
                return NotFound(new { message = "Token not found." });
            }
            
            _logger.LogInformation("User logged out");
            return Ok(new { message = "Logout successful. Client should dispose of all tokens." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error logging out user");
            return StatusCode(500, new { message = "An unexpected error occurred during logout." });
        }
    }



}

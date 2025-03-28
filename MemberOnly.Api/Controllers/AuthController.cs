using MemberOnly.Api.Models;
using MemberOnly.Api.Services;
using MemberOnly.Api.Services.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace MemberOnly.Api.Controllers;


using JWTClaims = System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames;
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly RefreshTokenService _refreshTokenService;
    private readonly TokenProvider _tokenProvider;
    private readonly ILogger<AuthController> _logger;
    
    public AuthController(RefreshTokenService refreshTokenService,
        ILogger<AuthController> logger , TokenProvider tokenProvider)
    {
        _refreshTokenService = refreshTokenService;
        _logger = logger;
        _tokenProvider = tokenProvider;
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

    
    
}
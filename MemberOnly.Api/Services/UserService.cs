using System;
using MemberOnly.Api.Database;
using MemberOnly.Api.Models;
using MemberOnly.Api.Services.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace MemberOnly.Api.Services;

public class UserService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<UserService> _logger;
    private readonly PasswordHasher _passwordHasher;
    private readonly TokenProvider _tokenProvider;

    private readonly TimeSpan _refreshTokenLifetime = TimeSpan.FromMinutes(5); 




    public UserService(ApplicationDbContext context, ILogger<UserService> logger , PasswordHasher passwordHasher, TokenProvider tokenProvider)
    {
        _context = context;
        _logger = logger;
        _passwordHasher = passwordHasher;
        _tokenProvider = tokenProvider;
    }

    public async Task<bool> UserNameCheckAsync(string username)
    {
        return await _context.Users.AnyAsync(x => x.Username == username);
    }


    public async Task<bool> UserHasCompletedActionAsync(string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
        if (user == null)
        {
            _logger.LogWarning("Attempt to check action completion for non-existent user: {Username}", username);
            throw new InvalidOperationException("User not found");
        }

        return user.HasCompletedAction;
    }
    
    public async Task UserCompletedActionAsync(string username)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == username);
        if (user == null)
        {
            _logger.LogWarning("Attempt to mark action as completed for non-existent user: {Username}", username);
            throw new InvalidOperationException("User not found");
        }

        user.HasCompletedAction = true;
        await _context.SaveChangesAsync();
    }
    

    public async Task<UserInfo> GetUserInfoAsync(string username)
    {
        var user = await _context.Users.Include(u => u.Posts).FirstOrDefaultAsync(x => x.Username == username);
        if (user == null)
        {
            _logger.LogWarning("Attempt to get info for non-existent user: {Username}", username);
            throw new InvalidOperationException("User not found");
        }

        return new UserInfo
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Username = user.Username,
            HasCompletedAction = user.HasCompletedAction,
            Posts = user.Posts  
        };
    }

    public async Task<UserRegister?> RegisterUserAsync(UserRegister UserReg)
    {
        if (await _context.Users.AnyAsync(x => x.Username == UserReg.Username))
        {
            _logger.LogWarning("Attempt to Register a user with duplicate username: {Username}", UserReg.Username);
            throw new InvalidOperationException("Username already exists");
        }

        var user = new User
        {
            Username = UserReg.Username,
            FirstName = UserReg.FirstName,
            LastName = UserReg.LastName,

        };

        user.PasswordHash = _passwordHasher.Hash(UserReg.Password);
        _context.Users.Add(user);

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is NpgsqlException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            _logger.LogError(ex, "Error saving new user {Username} to the database", user.Username);
            throw new Exception("The username is already in use.");
        }

        return UserReg;
    }

    // return a bearer token && refresh token 
    public async Task<Tokens> LoginUserAsync(UserLogin requestLogin)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == requestLogin.Username);
        if (user == null || !_passwordHasher.Verify(requestLogin.Password, user.PasswordHash))
        {
            _logger.LogWarning("Failed login attempt for {Username}", requestLogin.Username);
            throw new InvalidOperationException("Invalid username or password");
        }

        Tokens tokens = _tokenProvider.Create(user);


        var refreshTokenExpiry =   DateTime.UtcNow.Add(_refreshTokenLifetime);
     
        
        await _context.RefreshTokens.AddAsync(new UserRefreshTokens
        {
            UserName = user.Username,
            RefreshToken = tokens.RefreshToken,
            Expires = refreshTokenExpiry,
            IsActive = true
        });

        try {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Error saving refresh token to the database");
            throw new Exception("Error processing login. Please try again.");
        }

        return tokens;
    }
    
}
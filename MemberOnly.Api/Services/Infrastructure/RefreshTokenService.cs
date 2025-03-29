using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MemberOnly.Api.Database;
using MemberOnly.Api.Models;
using MemberOnly.Api.Services.Infrastructure;

namespace MemberOnly.Api.Services
{
    public class RefreshTokenService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<RefreshTokenService> _logger;
        private readonly TokenProvider _tokenProvider;
        private readonly TimeSpan _refreshTokenLifetime = TimeSpan.FromMinutes(5); 

        public RefreshTokenService(
            ApplicationDbContext context,
            ILogger<RefreshTokenService> logger,
            TokenProvider tokenProvider)
        {
            _context = context;
            _logger = logger;
            _tokenProvider = tokenProvider;
        }


        public async Task<Tokens> ValidateAndRotateTokenAsync(RefreshTokenFront refreshTokenRequest)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var existingToken = await _context.RefreshTokens
                    .Include(rt => rt.User)
                    .FirstOrDefaultAsync(rt => 
                        rt.UserName == refreshTokenRequest.UserName && 
                        rt.RefreshToken == refreshTokenRequest.RefreshToken);

                if (existingToken == null)
                {
                    throw new InvalidOperationException("Refresh token not found.");
                }

                if (!existingToken.IsActive)
                {
                    throw new InvalidOperationException("Refresh token has been revoked.");
                }

                if (existingToken.Expires < DateTime.UtcNow)
                {
                    throw new InvalidOperationException("Refresh token has expired.");
                }


                _context.RefreshTokens.Remove(existingToken);

                var user = existingToken.User;
                if (user == null)
                {
                    throw new InvalidOperationException("User not found.");
                }

                var tokens = _tokenProvider.Create(user);

                await _context.RefreshTokens.AddAsync(new UserRefreshTokens
                {
                    UserName = user.Username,
                    RefreshToken = tokens.RefreshToken,
                    Expires = DateTime.UtcNow.Add(_refreshTokenLifetime),
                    IsActive = true
                });

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return tokens;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

    
        public async Task<bool> DeleteRefreshTokenAsync(RefreshTokenFront refreshTokenRequest)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(rt => 
                rt.UserName == refreshTokenRequest.UserName && 
                rt.RefreshToken == refreshTokenRequest.RefreshToken);

            if (token == null)
            {
                return false;
            }

            _context.RefreshTokens.Remove(token);


            await _context.SaveChangesAsync();
            return true;
        }
    }
}

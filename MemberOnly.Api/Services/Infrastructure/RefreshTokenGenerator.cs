using System;
using System.Security.Cryptography;

namespace MemberOnly.Api.Services.Infrastructure
{
    public static class RefreshTokenGenerator
    {
        public static string Generate()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}
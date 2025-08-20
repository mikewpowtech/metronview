using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;

namespace Infrastructure.Identity
{
    public class CustomRefreshTokenProvider : IUserTwoFactorTokenProvider<ApplicationUserDb>
    {
        private readonly ITokenCacheService _tokenCache;
        private readonly ILogger<CustomRefreshTokenProvider> _logger;

        public CustomRefreshTokenProvider(ITokenCacheService tokenCache, ILogger<CustomRefreshTokenProvider> logger)
        {
            _tokenCache = tokenCache;
            _logger = logger;
        }

        public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<ApplicationUserDb> manager, ApplicationUserDb user)
            => Task.FromResult(false);

        public Task<string> GenerateAsync(string purpose, UserManager<ApplicationUserDb> manager, ApplicationUserDb user)
        {
            if (purpose != "RefreshToken")
                throw new ArgumentException("Invalid purpose", nameof(purpose));

            var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
            return Task.FromResult(token);
        }

        public async Task<bool> ValidateAsync(string purpose, string token, UserManager<ApplicationUserDb> manager, ApplicationUserDb user)
        {
            if (purpose != "RefreshToken")
                return false;

            return await _tokenCache.ValidateRefreshTokenAsync(user.Id, token);
        }
    }
}
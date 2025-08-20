using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

public interface ITokenCacheService
{
    Task<string?> GetRefreshTokenAsync(string userId);
    Task SetRefreshTokenAsync(string userId, string refreshToken, TimeSpan expiration);
    Task RemoveRefreshTokenAsync(string userId);
    Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken);
}

public class TokenCacheService : ITokenCacheService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<TokenCacheService> _logger;

    public TokenCacheService(IDistributedCache cache, ILogger<TokenCacheService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    public async Task<string?> GetRefreshTokenAsync(string userId)
    {
        try
        {
            return await _cache.GetStringAsync($"refresh_token:{userId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get refresh token for user {UserId}", userId);
            return null;
        }
    }

    public async Task SetRefreshTokenAsync(string userId, string refreshToken, TimeSpan expiration)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        };

        await _cache.SetStringAsync($"refresh_token:{userId}", refreshToken, options);
    }

    public async Task RemoveRefreshTokenAsync(string userId)
    {
        await _cache.RemoveAsync($"refresh_token:{userId}");
    }

    public async Task<bool> ValidateRefreshTokenAsync(string userId, string refreshToken)
    {
        var cachedToken = await GetRefreshTokenAsync(userId);
        return !string.IsNullOrEmpty(cachedToken) && cachedToken == refreshToken;
    }
}
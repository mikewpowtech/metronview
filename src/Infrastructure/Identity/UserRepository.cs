using Application;
using Application.Identity;
using Domain.Identity;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Identity
{
    public class UserRepository(UserManager<ApplicationUserDb> userManager,
        SignInManager<ApplicationUserDb> signInManager,
        ApplicationDbContext applicationDbContext, 
        IMapper mapper, 
        TokenSettings tokenSettings,
        ITokenCacheService tokenCache,
        ILogger<UserRepository> logger) : IUserRepository
    {
        public async Task<List<Claim>> GetUserClaimsAsync(string userId)
        {
            List<Claim> claimsResult = new List<Claim>();
            var claims = await (from ur in applicationDbContext.UserRoles
                          where ur.UserId == userId
                          join r in applicationDbContext.Roles on ur.RoleId equals r.Id
                          join rc in applicationDbContext.RoleClaims on r.Id equals rc.RoleId
                          select rc)
              .Where(rc => !string.IsNullOrEmpty(rc.ClaimValue) && !string.IsNullOrEmpty(rc.ClaimType))
              .Select(rc => new Claim(rc.ClaimType!, rc.ClaimValue!))
              .Distinct()
              .ToListAsync();

            claimsResult.AddRange(claims);

            var roleClaims = await (from ur in applicationDbContext.UserRoles
                              where ur.UserId == userId
                              join r in applicationDbContext.Roles on ur.RoleId equals r.Id
                              select r)
              .Where(r => !string.IsNullOrEmpty(r.Name))
              .Select(r => new Claim(ClaimTypes.Role, r.Name!))
              .Distinct()
              .ToListAsync();

            claimsResult.AddRange(roleClaims);

            return claimsResult;
        }

        public async Task<(SignInResult signinResult, string token, string refreshToken)> UserLoginAsync(string email, string password, bool lockoutOnFailure=false)
        {
            var dbUser = await userManager.FindByEmailAsync(email);
            if (dbUser is not null)
            {
                var result = await signInManager.CheckPasswordSignInAsync(dbUser, password, lockoutOnFailure);
                if (result.Succeeded)
                {
                    var tokens = await GenerateUserTokenAsync(dbUser);
                    return (result, tokens.token, tokens.refreshToken);
                }
            }
            return (SignInResult.Failed, null, null);
        }

        public async Task<IdentityResult?> CreateUserAsync(ApplicationUser user, string password)
        {
            var dbUser = mapper.Map<ApplicationUserDb>(user);
            var result = await userManager.CreateAsync(dbUser, password);
            return result;
        }

        private async Task<(string token, string refreshToken)> GenerateUserTokenAsync(ApplicationUserDb dbUser)
        {
            try
            {
                var claims = await GetUserClaimsAsync(dbUser.Id);
                
                // Remove any existing refresh token from cache
                await tokenCache.RemoveRefreshTokenAsync(dbUser.Id);
                
                // Generate JWT token
                var token = TokenUtil.GetToken(tokenSettings, dbUser.Id, dbUser?.UserName ?? "", claims);
                
                // Generate refresh token
                var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
                
                // Store refresh token in cache with expiration
                var refreshTokenExpiration = TimeSpan.FromDays(7); // Configure as needed
                await tokenCache.SetRefreshTokenAsync(dbUser.Id, refreshToken, refreshTokenExpiration);
                
                logger.LogInformation("Generated new tokens for user {UserId}", dbUser.Id);
                
                return (token, refreshToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to generate tokens for user {UserId}", dbUser.Id);
                throw;
            }
        }

        public async Task<(bool isSuccessful, string token, string refreshToken)> UserRefreshTokenAsync(string username, string refreshToken)
        {
            try
            {
                var dbUser = await userManager.FindByNameAsync(username);
                if (dbUser == null)
                {
                    logger.LogWarning("Refresh token attempt for non-existent user: {Username}", username);
                    return (false, null, null);
                }

                // Validate refresh token from cache instead of database
                if (!await tokenCache.ValidateRefreshTokenAsync(dbUser.Id, refreshToken))
                {
                    logger.LogWarning("Invalid refresh token for user {UserId}", dbUser.Id);
                    return (false, null, null);
                }

                var tokens = await GenerateUserTokenAsync(dbUser);
                return (true, tokens.token, tokens.refreshToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error refreshing token for user {Username}", username);
                return (false, null, null);
            }
        }

        public async Task<bool> UserLogoutAsync(string username)
        {
            try
            {
                var appUser = await applicationDbContext.Users.FirstOrDefaultAsync(x => x.UserName == username);
                if (appUser != null)
                {
                    // Remove refresh token from cache
                    await tokenCache.RemoveRefreshTokenAsync(appUser.Id);
                    
                    // Update security stamp to invalidate existing tokens
                    await userManager.UpdateSecurityStampAsync(appUser);
                    
                    logger.LogInformation("User {Username} logged out successfully", username);
                }
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during logout for user {Username}", username);
                return false;
            }
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            var response=mapper.Map<List<ApplicationUser>>(await applicationDbContext.Users.ToListAsync());
            return response;
        }
    }
}

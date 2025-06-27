using Application;
using Application.Identity;
using Domain.Identity;
using MapsterMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Infrastructure.Identity
{
    public class UserRepository(UserManager<ApplicationUserDb> userManager,
        SignInManager<ApplicationUserDb> signInManager,
        ApplicationDbContext applicationDbContext, IMapper mapper, TokenSettings tokenSettings) : IUserRepository
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

        public async Task<bool> UserLogoutAsync(string username)
        {
            var appUser = applicationDbContext.Users.First(x => x.UserName == username);
            if (appUser != null) { await userManager.UpdateSecurityStampAsync(appUser); }
            return true;
        }

        /// <summary>
        /// user login
        /// find the user by email, check password, if successful generate token and refresh token
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="lockoutOnFailure"></param>
        /// <returns></returns>
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

        public async Task<(bool isSuccessful,string token, string refreshToken)> UserRefreshTokenAsync(string username, string refreshToken)
        {
            var dbUser = await userManager.FindByNameAsync(username);
            if (dbUser == null) 
                return (false, null, null);

            if (!await userManager.VerifyUserTokenAsync(dbUser, "REFRESHTOKENPROVIDER", "RefreshToken", refreshToken))
                return (false, null, null);

            var tokens= await GenerateUserTokenAsync(dbUser);

            return (true, tokens.token, tokens.refreshToken);
        }

        public async Task<List<ApplicationUser>> GetAllUsersAsync()
        {
            var response=mapper.Map<List<ApplicationUser>>(await applicationDbContext.Users.ToListAsync());
            return response;
        }

        private async Task<(string token, string refreshToken)> GenerateUserTokenAsync(ApplicationUserDb dbUser)
        {
            var claims = await GetUserClaimsAsync(dbUser.Id);
            await userManager.RemoveAuthenticationTokenAsync(dbUser, "REFRESHTOKENPROVIDER", "RefreshToken");
            await userManager.UpdateAsync(dbUser);

            var token = TokenUtil.GetToken(tokenSettings, dbUser.Id, dbUser?.UserName ?? "", claims);
            var refreshToken = await userManager.GenerateUserTokenAsync(dbUser, "REFRESHTOKENPROVIDER", "RefreshToken");

            await userManager.SetAuthenticationTokenAsync(dbUser, "REFRESHTOKENPROVIDER", "RefreshToken", refreshToken);

            return (token, refreshToken);
        }
    }
}

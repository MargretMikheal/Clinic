using Clinic.Core.BLL.Interfaces;
using Clinic.Core.BLL.Auth.DTOs;
using Clinic.Core.BLL.Users.DTOs;
using Clinic.Core.Common.Responses;
using Clinic.Core.Data;
using Clinic.Core.Identity;
using Clinic.Core.Identity.Stores;
using Microsoft.AspNet.Identity;
using Microsoft.Owin;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Clinic.Core.BLL.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly Entities _context;

        public AuthenticationService()
        {
            _context = new Entities();
        }

        public async Task<BaseResponse<LoginResponse>> LoginAsync(LoginRequest request, IOwinContext context)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BaseResponse<LoginResponse>.Fail("Invalid request data.");
            }

            var store = new ApplicationUserStore(_context);
            var userManager = new UserManager<ApplicationUser>(store);

            var user = await userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                return BaseResponse<LoginResponse>.Fail("Invalid username or password.");
            }

            bool isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                return BaseResponse<LoginResponse>.Fail("Invalid username or password.");
            }

            var roles = await userManager.GetRolesAsync(user.Id);

            var identity = new ClaimsIdentity(DefaultAuthenticationTypes.ApplicationCookie);
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id));
            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));

            foreach (var role in roles)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, role));
            }

            // Sign in
            var authManager = context.Authentication;
            authManager.SignIn(new Microsoft.Owin.Security.AuthenticationProperties
            {
                IsPersistent = true
            }, identity);

            var responseData = new LoginResponse
            {
                UserId = user.Id,
                UserName = user.UserName,
                DisplayName = user.UserName,
                Role = roles.FirstOrDefault()
            };

            return BaseResponse<LoginResponse>.Success(responseData, "Login successful.");
        }

        public void Logout(IOwinContext context)
        {
            var authManager = context.Authentication;
            authManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        }

        public async Task<BaseResponse<UserInfo>> GetCurrentUserAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId))
            {
                return BaseResponse<UserInfo>.Fail("User ID is required.");
            }

            var store = new ApplicationUserStore(_context);
            var userManager = new UserManager<ApplicationUser>(store);

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BaseResponse<UserInfo>.Fail("User not found.");
            }

            var roles = await userManager.GetRolesAsync(user.Id);

            var userInfo = new UserInfo
            {
                UserId = user.Id,
                UserName = user.UserName,
                DisplayName = user.UserName,
                Role = roles.FirstOrDefault()
            };

            return BaseResponse<UserInfo>.Success(userInfo);
        }
    }
}

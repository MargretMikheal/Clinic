using Clinic.Core.BLL.Auth.DTOs;
using Clinic.Core.BLL.Users.DTOs;
using Clinic.Core.Common.Responses;
using Microsoft.Owin;
using System.Threading.Tasks;

namespace Clinic.Core.BLL.Interfaces
{
    public interface IAuthenticationService
    {
        Task<BaseResponse<LoginResponse>> LoginAsync(LoginRequest request, IOwinContext context);
        void Logout(IOwinContext context);
        Task<BaseResponse<UserInfo>> GetCurrentUserAsync(string userId);
    }
}

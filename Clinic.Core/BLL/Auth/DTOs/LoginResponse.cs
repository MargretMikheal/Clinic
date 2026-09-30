using System.Collections.Generic;

namespace Clinic.Core.BLL.Auth.DTOs
{
    public class LoginResponse
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public string Role { get; set; }
    }
}

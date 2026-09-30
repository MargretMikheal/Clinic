using System.ComponentModel.DataAnnotations;

namespace Clinic.Core.BLL.Auth.DTOs
{
    public class LoginRequest
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public string Password { get; set; }
    }
}

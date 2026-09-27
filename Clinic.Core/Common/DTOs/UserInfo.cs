using System.Collections.Generic;

namespace Clinic.Core.Common.DTOs
{
    public class UserInfo
    {
        public string UserId { get; set; }
        public string UserName { get; set; }
        public string DisplayName { get; set; }
        public List<string> Roles { get; set; }
    }
}

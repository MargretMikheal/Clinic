using System.Collections.Generic;

namespace Clinic.Core.BLL.RolePermission.DTOs
{
    public class SaveRoleScreenPermissionDto
    {
        public string RoleId { get; set; }
        public List<ScreenPermissionDto> Permissions { get; set; }
    }
}

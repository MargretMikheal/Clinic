namespace Clinic.Core.BLL.RolePermission.DTOs
{
    public class ScreenPermissionDto
    {
        public int ScreenId { get; set; }
        public string ScreenName { get; set; }
        public string Area { get; set; }
        public string Controller { get; set; }
        public string Action { get; set; }
        public string Icon { get; set; }
        public int? ParentId { get; set; }
        public bool ScreenShow { get; set; }
        public bool ScreenAdd { get; set; }
        public bool ScreenEdit { get; set; }
        public bool ScreenDelete { get; set; }
        public bool ScreenPrint { get; set; }
    }
}

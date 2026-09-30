using Clinic.Core.BLL.Dashboard.DTOs;
using System.Threading.Tasks;

namespace Clinic.Core.BLL.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardResponse> GetDashboardAsync();
    }
}

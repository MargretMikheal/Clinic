using Clinic.Core.Common.DTOs.Dashboard;
using System.Threading.Tasks;

namespace Clinic.Core.BLL.Interfaces
{
    public interface IAdminDashboardService
    {
        Task<AdminDashboardResponse> GetDashboardAsync();
    }
}
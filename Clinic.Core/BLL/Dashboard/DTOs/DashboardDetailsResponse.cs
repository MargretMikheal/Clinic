using System.Collections.Generic;

namespace Clinic.Core.BLL.Dashboard.DTOs
{
    public class DashboardDetailsResponse
    {
        public string Key { get; set; }

        public string LocalizationKey { get; set; }

        public int TotalCount { get; set; }

        public object Items { get; set; }

        public List<string> Columns { get; set; }

        public DashboardDetailsResponse()
        {
            Columns = new List<string>();
        }
    }
}

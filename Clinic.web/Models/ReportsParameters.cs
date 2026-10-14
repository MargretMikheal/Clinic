using System.Collections.Generic;

namespace Clinic.Web.Models
{
    public class ReportsParameters
    {
        private Dictionary<string, object> _params = new Dictionary<string, object>();

        public Dictionary<string, object> Parms
        {
            get { return _params; }
            set { _params = value; }
        }

        public string ReportName { get; set; }
    }
}

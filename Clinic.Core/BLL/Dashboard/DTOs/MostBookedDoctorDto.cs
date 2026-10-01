using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic.Core.BLL.Dashboard.DTOs
{
    public class MostBookedDoctorDto
    {
        public int DoctorId { get; set; }
        public string DoctorName { get; set; }
        public string DoctorCode { get; set; }
        public int AppointmentCount { get; set; }
    }
}

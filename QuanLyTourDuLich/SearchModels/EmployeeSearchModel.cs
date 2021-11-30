using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTourDuLich.SearchModels
{
    public class EmployeeSearchModel
    {
        public string EmpName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public DateTime? WorkingDate { get; set; }
    }
}

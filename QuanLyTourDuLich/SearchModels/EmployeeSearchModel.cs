using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTourDuLich.SearchModels
{
    public class EmployeeSearchModel
    {
        public Guid? empID { get; set; }
        public string empName { get; set; }
        public bool? gender { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
        public DateTime? workingDate { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTourDuLich.SearchModels
{
    public class CustomerSearchModel
    {
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; }
        public bool? Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
    }

    public class TransportSearchModel
    {
        public Guid? TransportTypeID { get; set; }
        public string CompanyName { get; set; }
    }

    public class CustomerUpdatePass
    {
        public Guid? CustomerId { get; set; }
        public string Password { get; set; }
        public string PasswordOld { get; set; }
    }
}

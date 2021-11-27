using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTourDuLich.SearchModels
{
    public class CustomerSearchModel
    {
        public Guid? customerId { get; set; }
        public string customerName { get; set; }
        public bool? gender { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }
    }
}

using QuanLyTourDuLich.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTourDuLich.SearchModels
{
    public class BookingTourModels : BookingTour
    {
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string Address { get; set; }
    }

    public class BookingTourSearch
    {
        public Guid? TourID { get; set; }
        public DateTime? BookingDate { get; set; }
        public bool? Status { get; set; }
    }

    public class BookingTourDelete
    {
        public Guid? BookingTourID { get; set; }
    }

    public class BookingSaerchCli
    {
        public Guid? CustomerId { get; set; }
        public bool isDelete { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
    }
}

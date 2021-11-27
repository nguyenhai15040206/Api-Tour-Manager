using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class BookingTour
    {
        public Guid BookingTourId { get; set; }
        public DateTime? BookingDate { get; set; }
        public Guid? TourId { get; set; }
        public Guid? CustomerId { get; set; }
        public int? QuanityAdult { get; set; }
        public int? QuanityChildren { get; set; }
        public int? QuanityBaby { get; set; }
        public decimal? TotalMoneyBooking { get; set; }
        public double? Discount { get; set; }
        public decimal? Surcharge { get; set; }
        public decimal? TotalMoney { get; set; }
        public Guid? EmpIdconfirm { get; set; }
        public string Note { get; set; }
        public bool? Status { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Employee EmpIdconfirmNavigation { get; set; }
        public virtual Tour Tour { get; set; }
    }
}

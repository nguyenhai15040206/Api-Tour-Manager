using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class Customer
    {
        public Customer()
        {
            BookingTour = new HashSet<BookingTour>();
            Comments = new HashSet<Comments>();
        }

        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; }
        public bool? Gender { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Password { get; set; }
        public Guid? EmpIdinsert { get; set; }
        public DateTime? DateInsert { get; set; }
        public Guid? EmpIdupdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Employee EmpIdinsertNavigation { get; set; }
        public virtual Employee EmpIdupdateNavigation { get; set; }
        public virtual ICollection<BookingTour> BookingTour { get; set; }
        public virtual ICollection<Comments> Comments { get; set; }
    }
}

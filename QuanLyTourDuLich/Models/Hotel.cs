using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class Hotel
    {
        public Hotel()
        {
            TourDetails = new HashSet<TourDetails>();
        }

        public Guid HotelId { get; set; }
        public string HotelName { get; set; }
        public int? Rating { get; set; }
        public string Introduce { get; set; }
        public int? RoomNumber { get; set; }
        public string Representative { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string ImagesList { get; set; }
        public int? WardId { get; set; }
        public Guid? HotelTypeId { get; set; }
        public Guid? EmpIdinsert { get; set; }
        public DateTime? DateInsert { get; set; }
        public Guid? EmpIdupdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public string Note { get; set; }
        public bool? Status { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Employee EmpIdinsertNavigation { get; set; }
        public virtual Employee EmpIdupdateNavigation { get; set; }
        public virtual HotelType HotelType { get; set; }
        public virtual Wards Ward { get; set; }
        public virtual ICollection<TourDetails> TourDetails { get; set; }
    }
}

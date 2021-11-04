using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class Tour
    {
        public Tour()
        {
            BookingTour = new HashSet<BookingTour>();
            Comments = new HashSet<Comments>();
            TourDetails = new HashSet<TourDetails>();
            UnitPrice = new HashSet<UnitPrice>();
        }

        public int TourId { get; set; }
        public string TourName { get; set; }
        public string Description { get; set; }
        public string TourImg { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public string PhuongTienXuatPhat { get; set; }
        public int? QuanityMax { get; set; }
        public int? QuanityMin { get; set; }
        public int? CurrentQuanity { get; set; }
        public string Schedule { get; set; }
        public int? Rating { get; set; }
        public int? DeparturePlace { get; set; }
        public int? TourGuideId { get; set; }
        public int? TravelTypeId { get; set; }
        public int? EmpIdinsert { get; set; }
        public DateTime? DateInsert { get; set; }
        public int? EmpIdupdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool? Suggest { get; set; }
        public bool? Status { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Province DeparturePlaceNavigation { get; set; }
        public virtual Employee EmpIdinsertNavigation { get; set; }
        public virtual Employee EmpIdupdateNavigation { get; set; }
        public virtual TourGuide TourGuide { get; set; }
        public virtual TravelType TravelType { get; set; }
        public virtual ICollection<BookingTour> BookingTour { get; set; }
        public virtual ICollection<Comments> Comments { get; set; }
        public virtual ICollection<TourDetails> TourDetails { get; set; }
        public virtual ICollection<UnitPrice> UnitPrice { get; set; }
    }
}

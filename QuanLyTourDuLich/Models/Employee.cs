using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class Employee
    {
        public Employee()
        {
            BookingTour = new HashSet<BookingTour>();
            Comments = new HashSet<Comments>();
            HotelEmpIdinsertNavigation = new HashSet<Hotel>();
            HotelEmpIdupdateNavigation = new HashSet<Hotel>();
            HotelTypeEmpIdinsertNavigation = new HashSet<HotelType>();
            HotelTypeEmpIdupdateNavigation = new HashSet<HotelType>();
            KindOfNewsEmpIdinsertNavigation = new HashSet<KindOfNews>();
            KindOfNewsEmpIdupdateNavigation = new HashSet<KindOfNews>();
            NewsEmpIdinsertNavigation = new HashSet<News>();
            NewsEmpIdupdateNavigation = new HashSet<News>();
            TourDetailsEmpIdinsertNavigation = new HashSet<TourDetails>();
            TourDetailsEmpIdupdateNavigation = new HashSet<TourDetails>();
            TourEmpIdinsertNavigation = new HashSet<Tour>();
            TourEmpIdupdateNavigation = new HashSet<Tour>();
            TourGuideEmpIdinsertNavigation = new HashSet<TourGuide>();
            TourGuideEmpIdupdateNavigation = new HashSet<TourGuide>();
            TouristAttractionEmpIdinsertNavigation = new HashSet<TouristAttraction>();
            TouristAttractionEmpIdupdateNavigation = new HashSet<TouristAttraction>();
            TravelTypeEmpIdinsertNavigation = new HashSet<TravelType>();
            TravelTypeEmpIdupdateNavigation = new HashSet<TravelType>();
            UnitPriceEmpIdinsertNavigation = new HashSet<UnitPrice>();
            UnitPriceEmpIdupdateNavigation = new HashSet<UnitPrice>();
        }

        public int EmpId { get; set; }
        public string EmpName { get; set; }
        public bool? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? WorkingDate { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Avatar { get; set; }
        public DateTime? DateInsert { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool? Status { get; set; }
        public bool? IsDelete { get; set; }

        public virtual ICollection<BookingTour> BookingTour { get; set; }
        public virtual ICollection<Comments> Comments { get; set; }
        public virtual ICollection<Hotel> HotelEmpIdinsertNavigation { get; set; }
        public virtual ICollection<Hotel> HotelEmpIdupdateNavigation { get; set; }
        public virtual ICollection<HotelType> HotelTypeEmpIdinsertNavigation { get; set; }
        public virtual ICollection<HotelType> HotelTypeEmpIdupdateNavigation { get; set; }
        public virtual ICollection<KindOfNews> KindOfNewsEmpIdinsertNavigation { get; set; }
        public virtual ICollection<KindOfNews> KindOfNewsEmpIdupdateNavigation { get; set; }
        public virtual ICollection<News> NewsEmpIdinsertNavigation { get; set; }
        public virtual ICollection<News> NewsEmpIdupdateNavigation { get; set; }
        public virtual ICollection<TourDetails> TourDetailsEmpIdinsertNavigation { get; set; }
        public virtual ICollection<TourDetails> TourDetailsEmpIdupdateNavigation { get; set; }
        public virtual ICollection<Tour> TourEmpIdinsertNavigation { get; set; }
        public virtual ICollection<Tour> TourEmpIdupdateNavigation { get; set; }
        public virtual ICollection<TourGuide> TourGuideEmpIdinsertNavigation { get; set; }
        public virtual ICollection<TourGuide> TourGuideEmpIdupdateNavigation { get; set; }
        public virtual ICollection<TouristAttraction> TouristAttractionEmpIdinsertNavigation { get; set; }
        public virtual ICollection<TouristAttraction> TouristAttractionEmpIdupdateNavigation { get; set; }
        public virtual ICollection<TravelType> TravelTypeEmpIdinsertNavigation { get; set; }
        public virtual ICollection<TravelType> TravelTypeEmpIdupdateNavigation { get; set; }
        public virtual ICollection<UnitPrice> UnitPriceEmpIdinsertNavigation { get; set; }
        public virtual ICollection<UnitPrice> UnitPriceEmpIdupdateNavigation { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class Employee
    {
        public Employee()
        {
            BookingTour = new HashSet<BookingTour>();
            CatEnumerationEmpIdinsertNavigation = new HashSet<CatEnumeration>();
            CatEnumerationEmpIdupdateNavigation = new HashSet<CatEnumeration>();
            Comments = new HashSet<Comments>();
            CustomerEmpIdinsertNavigation = new HashSet<Customer>();
            CustomerEmpIdupdateNavigation = new HashSet<Customer>();
            HotelEmpIdinsertNavigation = new HashSet<Hotel>();
            HotelEmpIdupdateNavigation = new HashSet<Hotel>();
            NewsEmpIdinsertNavigation = new HashSet<News>();
            NewsEmpIdupdateNavigation = new HashSet<News>();
            PromotionEmpIdinsertNavigation = new HashSet<Promotion>();
            PromotionEmpIdupdateNavigation = new HashSet<Promotion>();
            PromotionalTourEmpIdinsertNavigation = new HashSet<PromotionalTour>();
            PromotionalTourEmpIdupdateNavigation = new HashSet<PromotionalTour>();
            TourDetailsEmpIdinsertNavigation = new HashSet<TourDetails>();
            TourDetailsEmpIdupdateNavigation = new HashSet<TourDetails>();
            TourEmpIdinsertNavigation = new HashSet<Tour>();
            TourEmpIdupdateNavigation = new HashSet<Tour>();
            TourGuideEmpIdinsertNavigation = new HashSet<TourGuide>();
            TourGuideEmpIdupdateNavigation = new HashSet<TourGuide>();
            TouristAttractionEmpIdinsertNavigation = new HashSet<TouristAttraction>();
            TouristAttractionEmpIdupdateNavigation = new HashSet<TouristAttraction>();
            TravelCompanyTransportEmpIdinsertNavigation = new HashSet<TravelCompanyTransport>();
            TravelCompanyTransportEmpIdupdateNavigation = new HashSet<TravelCompanyTransport>();
        }

        public Guid EmpId { get; set; }
        public string EmpName { get; set; }
        public bool? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime? WorkingDate { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Avatar { get; set; }
        public string Address { get; set; }
        public DateTime? DateInsert { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool? Status { get; set; }
        public bool? IsDelete { get; set; }

        public virtual ICollection<BookingTour> BookingTour { get; set; }
        public virtual ICollection<CatEnumeration> CatEnumerationEmpIdinsertNavigation { get; set; }
        public virtual ICollection<CatEnumeration> CatEnumerationEmpIdupdateNavigation { get; set; }
        public virtual ICollection<Comments> Comments { get; set; }
        public virtual ICollection<Customer> CustomerEmpIdinsertNavigation { get; set; }
        public virtual ICollection<Customer> CustomerEmpIdupdateNavigation { get; set; }
        public virtual ICollection<Hotel> HotelEmpIdinsertNavigation { get; set; }
        public virtual ICollection<Hotel> HotelEmpIdupdateNavigation { get; set; }
        public virtual ICollection<News> NewsEmpIdinsertNavigation { get; set; }
        public virtual ICollection<News> NewsEmpIdupdateNavigation { get; set; }
        public virtual ICollection<Promotion> PromotionEmpIdinsertNavigation { get; set; }
        public virtual ICollection<Promotion> PromotionEmpIdupdateNavigation { get; set; }
        public virtual ICollection<PromotionalTour> PromotionalTourEmpIdinsertNavigation { get; set; }
        public virtual ICollection<PromotionalTour> PromotionalTourEmpIdupdateNavigation { get; set; }
        public virtual ICollection<TourDetails> TourDetailsEmpIdinsertNavigation { get; set; }
        public virtual ICollection<TourDetails> TourDetailsEmpIdupdateNavigation { get; set; }
        public virtual ICollection<Tour> TourEmpIdinsertNavigation { get; set; }
        public virtual ICollection<Tour> TourEmpIdupdateNavigation { get; set; }
        public virtual ICollection<TourGuide> TourGuideEmpIdinsertNavigation { get; set; }
        public virtual ICollection<TourGuide> TourGuideEmpIdupdateNavigation { get; set; }
        public virtual ICollection<TouristAttraction> TouristAttractionEmpIdinsertNavigation { get; set; }
        public virtual ICollection<TouristAttraction> TouristAttractionEmpIdupdateNavigation { get; set; }
        public virtual ICollection<TravelCompanyTransport> TravelCompanyTransportEmpIdinsertNavigation { get; set; }
        public virtual ICollection<TravelCompanyTransport> TravelCompanyTransportEmpIdupdateNavigation { get; set; }
    }
}

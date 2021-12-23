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
            PromotionalTour = new HashSet<PromotionalTour>();
            TourDetails = new HashSet<TourDetails>();
        }

        public Guid TourId { get; set; }
        public string TourName { get; set; }
        public string Description { get; set; }
        public string TourImg { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public int? Rating { get; set; }
        public int? QuanityMax { get; set; }
        public int? QuanityMin { get; set; }
        public int? CurrentQuanity { get; set; }
        public decimal? AdultUnitPrice { get; set; }
        public decimal? ChildrenUnitPrice { get; set; }
        public decimal? BabyUnitPrice { get; set; }
        public decimal? Surcharge { get; set; }
        public string Schedule { get; set; }
        public int? DeparturePlaceFrom { get; set; }
        public int? DeparturePlaceTo { get; set; }
        public Guid? CompanyTransportStartId { get; set; }
        public Guid? CompanyTransportInTourId { get; set; }
        public Guid? TourGuideId { get; set; }
        public Guid? TravelTypeId { get; set; }
        public bool? Suggest { get; set; }
        public string NoteByTour { get; set; }
        public string ConditionByTour { get; set; }
        public string NoteByMyTour { get; set; }
        public Guid? EmpIdinsert { get; set; }
        public DateTime? DateInsert { get; set; }
        public Guid? EmpIdupdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool? IsDelete { get; set; }
        public int? GroupNumber { get; set; }

        public virtual TravelCompanyTransport CompanyTransportInTour { get; set; }
        public virtual TravelCompanyTransport CompanyTransportStart { get; set; }
        public virtual Province DeparturePlaceFromNavigation { get; set; }
        public virtual Province DeparturePlaceToNavigation { get; set; }
        public virtual Employee EmpIdinsertNavigation { get; set; }
        public virtual Employee EmpIdupdateNavigation { get; set; }
        public virtual TourGuide TourGuide { get; set; }
        public virtual CatEnumeration TravelType { get; set; }
        public virtual ICollection<BookingTour> BookingTour { get; set; }
        public virtual ICollection<Comments> Comments { get; set; }
        public virtual ICollection<PromotionalTour> PromotionalTour { get; set; }
        public virtual ICollection<TourDetails> TourDetails { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class UnitPriceTransport
    {
        public UnitPriceTransport()
        {
            TourUpTransportIdendNavigation = new HashSet<Tour>();
            TourUpTransportIdstartNavigation = new HashSet<Tour>();
        }

        public Guid UpTransportId { get; set; }
        public TimeSpan? TimeStart { get; set; }
        public TimeSpan? TimeEnd { get; set; }
        public int? ProvinceFrom { get; set; }
        public int? ProvinceTo { get; set; }
        public Guid? CompanyId { get; set; }
        public decimal? AdultUnitPrice { get; set; }
        public decimal? ChildrenUnitPrice { get; set; }
        public decimal? BabyUnitPrice { get; set; }
        public Guid? EmpIdinsert { get; set; }
        public DateTime? DateInsert { get; set; }
        public Guid? EmpIdupdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool? IsDelete { get; set; }

        public virtual TravelCompanyTransport Company { get; set; }
        public virtual Employee EmpIdinsertNavigation { get; set; }
        public virtual Employee EmpIdupdateNavigation { get; set; }
        public virtual Province ProvinceFromNavigation { get; set; }
        public virtual Province ProvinceToNavigation { get; set; }
        public virtual ICollection<Tour> TourUpTransportIdendNavigation { get; set; }
        public virtual ICollection<Tour> TourUpTransportIdstartNavigation { get; set; }
    }
}

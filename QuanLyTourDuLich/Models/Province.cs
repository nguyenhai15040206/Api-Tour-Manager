using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class Province
    {
        public Province()
        {
            District = new HashSet<District>();
            TourDeparturePlaceFromNavigation = new HashSet<Tour>();
            TourDeparturePlaceToNavigation = new HashSet<Tour>();
            TouristAttraction = new HashSet<TouristAttraction>();
            TravelCompanyTransport = new HashSet<TravelCompanyTransport>();
            UnitPriceTransportProvinceFromNavigation = new HashSet<UnitPriceTransport>();
            UnitPriceTransportProvinceToNavigation = new HashSet<UnitPriceTransport>();
        }

        public int ProvinceId { get; set; }
        public string ProvinceName { get; set; }
        public string DivisionType { get; set; }
        public int? Regions { get; set; }

        public virtual ICollection<District> District { get; set; }
        public virtual ICollection<Tour> TourDeparturePlaceFromNavigation { get; set; }
        public virtual ICollection<Tour> TourDeparturePlaceToNavigation { get; set; }
        public virtual ICollection<TouristAttraction> TouristAttraction { get; set; }
        public virtual ICollection<TravelCompanyTransport> TravelCompanyTransport { get; set; }
        public virtual ICollection<UnitPriceTransport> UnitPriceTransportProvinceFromNavigation { get; set; }
        public virtual ICollection<UnitPriceTransport> UnitPriceTransportProvinceToNavigation { get; set; }
    }
}

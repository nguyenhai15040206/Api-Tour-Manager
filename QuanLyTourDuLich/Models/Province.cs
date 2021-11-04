using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class Province
    {
        public Province()
        {
            District = new HashSet<District>();
            Tour = new HashSet<Tour>();
            TouristAttraction = new HashSet<TouristAttraction>();
        }

        public int ProvinceId { get; set; }
        public string ProvinceName { get; set; }
        public string DivisionType { get; set; }
        public int? PhoneCode { get; set; }

        public virtual ICollection<District> District { get; set; }
        public virtual ICollection<Tour> Tour { get; set; }
        public virtual ICollection<TouristAttraction> TouristAttraction { get; set; }
    }
}

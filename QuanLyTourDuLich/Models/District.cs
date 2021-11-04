using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class District
    {
        public District()
        {
            Wards = new HashSet<Wards>();
        }

        public int DistrictId { get; set; }
        public string DistrictName { get; set; }
        public string DivisionType { get; set; }
        public int? ProvinceId { get; set; }

        public virtual Province Province { get; set; }
        public virtual ICollection<Wards> Wards { get; set; }
    }
}

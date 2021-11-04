using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class Wards
    {
        public Wards()
        {
            Hotel = new HashSet<Hotel>();
        }

        public int WardId { get; set; }
        public string WardName { get; set; }
        public string DivisionType { get; set; }
        public int? DistrictId { get; set; }

        public virtual District District { get; set; }
        public virtual ICollection<Hotel> Hotel { get; set; }
    }
}

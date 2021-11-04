using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class HotelType
    {
        public HotelType()
        {
            Hotel = new HashSet<Hotel>();
        }

        public int HotelTypeId { get; set; }
        public string HotelTypeName { get; set; }
        public string Description { get; set; }
        public string Image { get; set; }
        public int? EmpIdinsert { get; set; }
        public DateTime? DateInsert { get; set; }
        public int? EmpIdupdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool? Status { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Employee EmpIdinsertNavigation { get; set; }
        public virtual Employee EmpIdupdateNavigation { get; set; }
        public virtual ICollection<Hotel> Hotel { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class TourDetails
    {
        public int TourId { get; set; }
        public int TouristAttrId { get; set; }
        public int? HotelId { get; set; }
        public int? EmpIdinsert { get; set; }
        public DateTime? DateInsert { get; set; }
        public int? EmpIdupdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Employee EmpIdinsertNavigation { get; set; }
        public virtual Employee EmpIdupdateNavigation { get; set; }
        public virtual Hotel Hotel { get; set; }
        public virtual Tour Tour { get; set; }
        public virtual TouristAttraction TouristAttr { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class TouristAttraction
    {
        public TouristAttraction()
        {
            TourDetails = new HashSet<TourDetails>();
        }

        public Guid TouristAttrId { get; set; }
        public string TouristAttrName { get; set; }
        public string Description { get; set; }
        public string ImagesList { get; set; }
        public Guid? EmpIdinsert { get; set; }
        public DateTime? DateInsert { get; set; }
        public Guid? EmpIdupdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public int? ProvinceId { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Employee EmpIdinsertNavigation { get; set; }
        public virtual Employee EmpIdupdateNavigation { get; set; }
        public virtual Province Province { get; set; }
        public virtual ICollection<TourDetails> TourDetails { get; set; }
    }
}

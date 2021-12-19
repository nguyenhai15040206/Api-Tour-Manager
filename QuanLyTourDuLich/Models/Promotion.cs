using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class Promotion
    {
        public Promotion()
        {
            PromotionalTour = new HashSet<PromotionalTour>();
        }

        public Guid PromotionId { get; set; }
        public string PromotionName { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public double? Discount { get; set; }
        public bool? IsApplyAll { get; set; }
        public Guid? EmpIdinsert { get; set; }
        public DateTime? DateInsert { get; set; }
        public Guid? EmpIdupdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Employee EmpIdinsertNavigation { get; set; }
        public virtual Employee EmpIdupdateNavigation { get; set; }
        public virtual ICollection<PromotionalTour> PromotionalTour { get; set; }
    }
}

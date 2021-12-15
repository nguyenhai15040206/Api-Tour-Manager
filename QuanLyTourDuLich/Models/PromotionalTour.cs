using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class PromotionalTour
    {
        public Guid TourId { get; set; }
        public Guid PromotionId { get; set; }
        public Guid? EmpIdinsert { get; set; }
        public DateTime? DateInsert { get; set; }
        public Guid? EmpIdupdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Employee EmpIdinsertNavigation { get; set; }
        public virtual Employee EmpIdupdateNavigation { get; set; }
        public virtual Promotion Promotion { get; set; }
        public virtual Tour Tour { get; set; }
    }
}

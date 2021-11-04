using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class UnitPrice
    {
        public int TourId { get; set; }
        public DateTime DateUpdate { get; set; }
        public decimal? AdultUnitPrice { get; set; }
        public decimal? ChildrenUnitPrice { get; set; }
        public decimal? BabyUnitPrice { get; set; }
        public decimal? Surcharge { get; set; }
        public double? Discount { get; set; }
        public int? EmpIdinsert { get; set; }
        public int? EmpIdupdate { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Employee EmpIdinsertNavigation { get; set; }
        public virtual Employee EmpIdupdateNavigation { get; set; }
        public virtual Tour Tour { get; set; }
    }
}

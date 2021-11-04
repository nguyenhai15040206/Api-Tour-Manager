using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class TravelType
    {
        public TravelType()
        {
            Tour = new HashSet<Tour>();
        }

        public int TravelTypeId { get; set; }
        public string TravelTypeName { get; set; }
        public string Note { get; set; }
        public int? EmpIdinsert { get; set; }
        public DateTime? DateInsert { get; set; }
        public int? EmpIdupdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool? Status { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Employee EmpIdinsertNavigation { get; set; }
        public virtual Employee EmpIdupdateNavigation { get; set; }
        public virtual ICollection<Tour> Tour { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class Comments
    {
        public int CommentId { get; set; }
        public DateTime? CommentDate { get; set; }
        public int? CustomerId { get; set; }
        public int? TourId { get; set; }
        public string Content { get; set; }
        public string ImagesList { get; set; }
        public int? EmpIdactive { get; set; }
        public DateTime? DateActive { get; set; }
        public int? EmpIdupdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool? Active { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Employee EmpIdactiveNavigation { get; set; }
        public virtual Tour Tour { get; set; }
    }
}

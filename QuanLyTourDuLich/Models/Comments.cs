using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class Comments
    {
        public Guid CommentId { get; set; }
        public DateTime? CommentDate { get; set; }
        public Guid? CustomerId { get; set; }
        public Guid? TourId { get; set; }
        public string Content { get; set; }
        public string ImagesList { get; set; }
        public Guid? EmpIdinsert { get; set; }
        public DateTime? DateActive { get; set; }
        public Guid? EmpIdupdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool? Active { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual Employee EmpIdinsertNavigation { get; set; }
        public virtual Tour Tour { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class News
    {
        public Guid NewsId { get; set; }
        public string NewsName { get; set; }
        public string Content { get; set; }
        public string NewsImg { get; set; }
        public string ImagesList { get; set; }
        public Guid? EmpIdinsert { get; set; }
        public DateTime? DateInsert { get; set; }
        public Guid? EmpIdupdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public Guid? EnumerationId { get; set; }
        public bool? Active { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Employee EmpIdinsertNavigation { get; set; }
        public virtual Employee EmpIdupdateNavigation { get; set; }
        public virtual CatEnumeration Enumeration { get; set; }
    }
}

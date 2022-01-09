using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class Banner
    {
        public Guid BannerId { get; set; }
        public string BannerImg { get; set; }
        public Guid? EnumerationId { get; set; }
        public bool? Active { get; set; }
        public Guid? EmpIdinsert { get; set; }
        public DateTime? DateInsert { get; set; }
        public Guid? EmpIdupdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Employee EmpIdinsertNavigation { get; set; }
        public virtual Employee EmpIdupdateNavigation { get; set; }
        public virtual CatEnumeration Enumeration { get; set; }
    }
}

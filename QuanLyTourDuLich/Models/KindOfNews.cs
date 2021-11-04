using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class KindOfNews
    {
        public KindOfNews()
        {
            News = new HashSet<News>();
        }

        public int KindOfNewsId { get; set; }
        public string KindOfNewsName { get; set; }
        public int? EmpIdinsert { get; set; }
        public DateTime? DateInsert { get; set; }
        public int? EmpIdupdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool? Status { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Employee EmpIdinsertNavigation { get; set; }
        public virtual Employee EmpIdupdateNavigation { get; set; }
        public virtual ICollection<News> News { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class CatEnumeration
    {
        public CatEnumeration()
        {
            Hotel = new HashSet<Hotel>();
            News = new HashSet<News>();
            Tour = new HashSet<Tour>();
            TravelCompanyTransport = new HashSet<TravelCompanyTransport>();
        }

        public Guid EnumerationId { get; set; }
        public string EnumerationType { get; set; }
        public string EnumerationName { get; set; }
        public string EnumerationTranslate { get; set; }
        public Guid? EmpIdinsert { get; set; }
        public DateTime? DateInsert { get; set; }
        public Guid? EmpIdupdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Employee EmpIdinsertNavigation { get; set; }
        public virtual Employee EmpIdupdateNavigation { get; set; }
        public virtual ICollection<Hotel> Hotel { get; set; }
        public virtual ICollection<News> News { get; set; }
        public virtual ICollection<Tour> Tour { get; set; }
        public virtual ICollection<TravelCompanyTransport> TravelCompanyTransport { get; set; }
    }
}

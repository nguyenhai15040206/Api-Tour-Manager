using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class TravelCompanyTransport
    {
        public TravelCompanyTransport()
        {
            Tour = new HashSet<Tour>();
            UnitPriceTransport = new HashSet<UnitPriceTransport>();
        }

        public Guid CompanyId { get; set; }
        public string CompanyName { get; set; }
        public string CompanyImage { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public int? ProvinceId { get; set; }
        public Guid? EnumerationId { get; set; }
        public Guid? EmpIdinsert { get; set; }
        public DateTime? DateInsert { get; set; }
        public Guid? EmpIdupdate { get; set; }
        public DateTime? DateUpdate { get; set; }
        public bool? IsDelete { get; set; }

        public virtual Employee EmpIdinsertNavigation { get; set; }
        public virtual Employee EmpIdupdateNavigation { get; set; }
        public virtual CatEnumeration Enumeration { get; set; }
        public virtual Province Province { get; set; }
        public virtual ICollection<Tour> Tour { get; set; }
        public virtual ICollection<UnitPriceTransport> UnitPriceTransport { get; set; }
    }
}

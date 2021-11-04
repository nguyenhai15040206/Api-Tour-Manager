using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class TourGuide
    {
        public TourGuide()
        {
            Tour = new HashSet<Tour>();
        }

        public int TourGuideId { get; set; }
        public string TourGuideName { get; set; }
        public bool? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string Avatar { get; set; }
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

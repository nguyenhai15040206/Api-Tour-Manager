using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTourDuLich.SearchModels
{
    public class TourGuideSearchModel
    {
        public Guid? TouGuideId { get; set; }
        public bool? Gender { get; set; }
        public string TourGuideName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }

    }
}

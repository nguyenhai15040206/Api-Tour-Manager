using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTourDuLich.SearchModels
{
    public class TourGuideSearchModel
    {
        public int? touGuideId { get; set; }
        public bool? gender { get; set; }
        public string tourGuideName { get; set; }
        public string phoneNumber { get; set; }
        public string email { get; set; }

    }
}

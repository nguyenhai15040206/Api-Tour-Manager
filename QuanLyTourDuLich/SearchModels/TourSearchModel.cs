using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTourDuLich.SearchModels
{
    public class TourSearchModel
    {

        // Nguyễn Tấn Hải - 20011108
        public string TourName { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }

        // loại tour
        public Guid? TravelTypeID { get; set; }

        // địa điểm xuất phát
        public int?[] DeparturePlace { get; set; }

        public bool? Suggest { get; set; }

    }


}

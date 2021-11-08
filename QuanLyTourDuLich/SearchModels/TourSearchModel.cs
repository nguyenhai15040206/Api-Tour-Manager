using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTourDuLich.SearchModels
{
    public class TourSearchModel
    {

        // Nguyễn Tấn Hải - 20011108
        public int? TourID { get; set; }
        public string TourName { get; set; }
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }

        // Phương tiện di chuyện
        public string Transport { get; set; }
        public int? Rating { get; set; }

        // địa điểm xuất phát
        public int? DeparturePlace { get; set; }

    }


}

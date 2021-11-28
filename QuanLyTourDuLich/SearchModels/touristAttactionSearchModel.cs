using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTourDuLich.SearchModels
{
    public class touristAttactionSearchModel
    {
        public Guid? TouristAttrID { get; set; }
        public string TouristAttrName { get; set; }
        public int? []ProvinceID { get; set; }
    }
}

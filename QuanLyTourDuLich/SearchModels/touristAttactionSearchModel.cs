using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTourDuLich.SearchModels
{
    public class touristAttactionSearchModel
    {
        public Guid? touristAttrID { get; set; }
        public string touristAttrName { get; set; }
        public Guid? []provinceID { get; set; }
    }
}

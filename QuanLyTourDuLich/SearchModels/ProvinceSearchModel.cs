using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTourDuLich.SearchModels
{
    public class ProvinceSearchModel
    {
        public Guid? []provinceID { get; set; }
        public string provinceName { get; set; }
        public string divisionType { get; set; }
    }
}

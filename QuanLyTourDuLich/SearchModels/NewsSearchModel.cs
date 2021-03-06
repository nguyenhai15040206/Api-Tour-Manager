using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTourDuLich.SearchModels
{
    public class NewsSearchModel
    {
        public string NewsName { get; set; }
        public Guid? KindOfNewsID { get; set; }
        public DateTime? DateUpdate { get; set; }
    }

    public class NewsSearchClientModels : NewsSearchModel
    {
        public bool MainPage { get; set; }
        public int Page { get; set; }
        public int Limit { get; set; }
    }
}

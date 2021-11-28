using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTourDuLich.SearchModels
{
    public class NewsSearchModel
    {
        public Guid? NewsId { get; set; }
        public string NewsName { get; set; }
        public Guid? KindOfNewsId { get; set; }
    }
}

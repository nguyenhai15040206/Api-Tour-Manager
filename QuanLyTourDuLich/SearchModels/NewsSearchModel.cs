using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTourDuLich.SearchModels
{
    public class NewsSearchModel
    {
        public Guid? newsId { get; set; }
        public string newsName { get; set; }
        public Guid? kindOfNewsId { get; set; }
    }
}

using QuanLyTourDuLich.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTourDuLich.SearchModels
{
    public class PromotionSearchModel
    {
        public string PromotionName{ get; set; }
        public bool? IsApplyAll{ get; set; }
    }

    public class PromotionModels: Promotion
    {
        public List<Guid> TourList { get; set; }
    }

    public class UnitPriceSearch
    {
        public Guid? TravelTypeID { get; set; }
        public Guid? CompanyID { get; set; }
        public int? ProvinceFrom { get; set; }
        public int? ProvinceTo { get; set; }
    }
}

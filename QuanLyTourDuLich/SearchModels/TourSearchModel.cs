using Newtonsoft.Json;
using QuanLyTourDuLich.Models;
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
        public bool? TourIsExpired { get; set; }

    }

    public class TourSearchModelClient
    {
        public Guid? TravelTypeID { get; set; }
        public int? DeparturePlaceFrom { get; set; }
        public int? DeparturePlaceTo { get; set; }
        public DateTime? DateStart { get; set; }
        
        // mb
        public string DeparturePlaceToName { get; set; }

        //
        public bool? QuantityDate1 { get; set; }
        public bool? QuantityDate2 { get; set; }
        public bool? QuantityDate3 { get; set; }
        public bool? QuantityDate4 { get; set; }

        //
        public bool? QuantityPeople1 { get; set; }
        public bool? QuantityPeople2 { get; set; }
        public bool? QuantityPeople3 { get; set; }
        public bool? QuantityPeople4 { get; set; }

        //
        public bool? TransportType1 { get; set; }
        public bool? TransportType2 { get; set; }

        public int Page { get; set; }
        public int Limit { get; set; }

    }

    public class TourModel
    {
        public Guid TourId { get; set; }
        public string TourName { get; set; }
        public string TourImg { get; set; }
        public DateTime? DateStart { get; set; }
        public string DateStartFormat { get; set; }
        public DateTime? DateEnd { get; set; }
        public int? Rating { get; set; }
        public int? QuanityMax { get; set; }
        public int? QuanityMin { get; set; }
        public int? CurrentQuanity { get; set; }
        public int? GroupNumber { get; set; }
        public decimal? AdultUnitPrice { get; set; }
        public decimal? ChildrenUnitPrice { get; set; }
        public decimal? BabyUnitPrice { get; set; }
        public string DeparturePlaceFromName { get; set; }
        public string DeparturePlaceToName { get; set; }
        public int? DeparturePlaceFrom { get; set; }
        public int? DeparturePlaceTo { get; set; }
        public int? TotalCurrentQuanity { get; set; }
        public int? TotalDays { get; set; }
        public Guid? TravelTypeId { get; set; }
        public string TravelTypeName { get; set; }
        public double? Promotion { get; set; }
    }
}

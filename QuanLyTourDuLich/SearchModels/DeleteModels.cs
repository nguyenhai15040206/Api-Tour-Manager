using QuanLyTourDuLich.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTourDuLich.SearchModels
{
    public class DeleteModels
    {
        public Guid?[] SelectByIds { get; set; }
        public Guid? EmpId { get; set; }
    }

    public class UpdateTourDetailsModels
    {
        public Guid TourID { get; set; }
        public Guid[] TourAttrIds {get;set;}
        public Guid? EmpId { get; set; }
    }
}

﻿using QuanLyTourDuLich.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTourDuLich.SearchModels
{
    public class BookingTourModels : BookingTour
    {
        public string CustomerName { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhone { get; set; }
        public string Address { get; set; }
    }

    public class BookingTourSearch
    {
        public Guid? TourID { get; set; }
        public DateTime? BookingDate { get; set; }
        public bool? Status { get; set; }
    }
}

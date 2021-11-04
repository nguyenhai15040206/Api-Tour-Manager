using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLyTourDuLich
{
    public class Pagination
    {
        public int currentPage { get; set; }
        public int count { get; set; }
        public int pagsize { get; set; }
        public int totalPage { get; set; }

        public int indexOne { get; set; }
        public int indexTwo { get; set; }

        public bool showPrevious => currentPage > 1;
        public bool showFirst => currentPage != 1;
        public bool showLast => currentPage != totalPage;
    }
    // Thái Trần Kiều Diêmx
}

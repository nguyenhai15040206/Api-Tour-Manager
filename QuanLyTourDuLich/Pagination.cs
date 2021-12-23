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
    public static class ValidateInput
    {
        public static bool Regex(string input)
        {
            foreach (char c in input)
            {
                if (c < '0' || c > '9')
                {
                    return false;
                }
            }
            return true;
        }

        // kiểm tra số điện thoại
        public static bool IsPhoneNumber(string input)
        {
            if (Regex(input) && input.Length == 10 && input[0] == '0')
            {
                return true;
            }
            return false;
        }
    }
}

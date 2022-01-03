using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class CatScreen
    {
        public CatScreen()
        {
            Permission = new HashSet<Permission>();
        }

        public Guid ScreenId { get; set; }
        public string ScreenName { get; set; }

        public virtual ICollection<Permission> Permission { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class Permission
    {
        public Guid UserGroupId { get; set; }
        public Guid ScreenId { get; set; }
        public bool? Status { get; set; }

        public virtual CatScreen Screen { get; set; }
        public virtual UserGroup UserGroup { get; set; }
    }
}

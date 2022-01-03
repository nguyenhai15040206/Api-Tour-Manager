using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class EmpUserGroup
    {
        public Guid EmpId { get; set; }
        public Guid UserGroupId { get; set; }

        public virtual Employee Emp { get; set; }
        public virtual UserGroup UserGroup { get; set; }
    }
}

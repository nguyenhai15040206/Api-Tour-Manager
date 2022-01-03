using System;
using System.Collections.Generic;

namespace QuanLyTourDuLich.Models
{
    public partial class UserGroup
    {
        public UserGroup()
        {
            EmpUserGroup = new HashSet<EmpUserGroup>();
            Permission = new HashSet<Permission>();
        }

        public Guid UserGroupId { get; set; }
        public string UserGroupName { get; set; }

        public virtual ICollection<EmpUserGroup> EmpUserGroup { get; set; }
        public virtual ICollection<Permission> Permission { get; set; }
    }
}

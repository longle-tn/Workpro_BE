using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Container_App.Core.Model.RolePermissions
{
    public class RolePermission
    {
        public Guid Id { get; set; }
        public Guid RoleId { get; set; }
        public Guid ResourceId { get; set; }
        public Guid PermissionId { get; set; }
    }
}

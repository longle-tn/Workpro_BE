using Container_App.Core.Model.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Container_App.Core.Interface.Permissions
{
    public interface IPermissionService
    {
        Task<IEnumerable<Permission>> GetListPermissionByUser(Guid userId);
        bool HasPermission( IEnumerable<Permission> userPermissions,string resource, string action);
    }
}

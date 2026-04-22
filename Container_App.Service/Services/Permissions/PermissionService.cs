using Container_App.Common.Shared;
using Container_App.Core.Interface.Permissions;
using Container_App.Core.Model.Permissions;
using Container_App.Core.Model.Users;
using Container_App.Data.Connection;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Container_App.Service.Services.Permissions
{
    public class PermissionService: IPermissionService
    {
        private readonly IStoredProcedureExecutor _storedProcedureExecutor;
        public PermissionService(IStoredProcedureExecutor storedProcedureExecutor)
        {
            _storedProcedureExecutor = storedProcedureExecutor;
        }

        public async Task<IEnumerable<Permission>> GetListPermissionByUser(Guid userId)
        { 
            try
            {             
                var arr = new[]
                {
                    new SqlParameter("@UserId", userId),               
                };
                return await _storedProcedureExecutor.QueryAsync<Permission>("sp_GetListPermissionByUser", arr);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message,
                "Error when get list role by user");

                throw;
            }
    }

        public bool HasPermission(IEnumerable<Permission> userPermissions, string resource, string action)
        {
            return userPermissions.Any(p =>
                p.ResourceName.Equals(resource, StringComparison.OrdinalIgnoreCase)
                && p.Action.Equals(action, StringComparison.OrdinalIgnoreCase)
            );
        }
    }
}

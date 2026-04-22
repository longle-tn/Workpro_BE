using Container_App.Common.Shared;
using Container_App.Core.Interface.RolePermissions;
using Container_App.Core.Model.RolePermissions;
using Container_App.Data.Connection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Container_App.Service.Services.RolePermissions
{
    public class RolePermissionService : IRolePermissionService
    {
        private readonly IStoredProcedureExecutor _procedureExecutor;
        public RolePermissionService(IStoredProcedureExecutor procedureExecutor)
        {
            _procedureExecutor = procedureExecutor;
        }

        public async Task<int> Insert(Guid roleId, List<RolePermission> lst)
        {
            try
            {
                var tb = new DataTable();
                tb.Columns.Add("ResourceId", typeof(Guid));
                tb.Columns.Add("PermissionId", typeof (Guid));
                foreach(var item in lst)
                {
                    tb.Rows.Add(item.ResourceId, item.PermissionId);
                }
                var arr = new[]
                {
                    new SqlParameter("@RoleId", roleId),
                    new SqlParameter("@ListResourcePermissions", SqlDbType.Structured)
                    {
                        TypeName = "resources_permissions",
                        Value = tb
                    }
                };
                return await _procedureExecutor.ExecuteAsync("sp_PermissionInRole", arr);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message,
                "Error when insert role permission. RoleId={roleId}", roleId);

                throw;
            }
        }
    }
}

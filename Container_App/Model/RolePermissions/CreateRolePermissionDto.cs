using Container_App.Core.Model.RolePermissions;

namespace Container_App.Model.RolePermissions
{
    public class CreateRolePermissionDto
    {
        public Guid RoleId { get; set; }
        public List<RolePermission> RolePermissions { get; set; }
    }
}

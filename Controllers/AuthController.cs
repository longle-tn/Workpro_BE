using Container_App.Model.Permissions;
using Container_App.Model.RoleMenuAccess;
using Container_App.Model.RolePermissions;
using Container_App.Model.Roles;
using Container_App.Model.UserRoles;
using Container_App.Model.Users;
using Container_App.Services.AuthService;
using Container_App.Services.MenuService;
using Container_App.Services.PermissionService;
using Container_App.Services.RoleMenuAccessService;
using Container_App.Services.RolePermissionsService;
using Container_App.Services.RoleService;
using Container_App.Services.UserRoleService;
using Container_App.utilities;
using Microsoft.AspNetCore.Mvc;

namespace Container_App.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IRoleService _roleService;
        private readonly IPermissionService _permissionService;
        private readonly IRolePermissionsService _rolePermissionsService;
        private readonly IUserRoleService _userRoleService;
        private readonly IRoleMenuAccessService _roleMenuAccessService;
        private readonly IMenuService _menuService;
        public AuthController(IAuthService authService, IRoleService roleService, IPermissionService permissionService,
            IRolePermissionsService rolePermissionsService , IUserRoleService userRoleService, IRoleMenuAccessService roleMenuAccessService,
            IMenuService menuService)
        {
            _authService = authService;
            _roleService = roleService;
            _permissionService = permissionService;
            _rolePermissionsService = rolePermissionsService;
            _userRoleService = userRoleService;
            _roleMenuAccessService = roleMenuAccessService;
            _menuService = menuService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel request)
        {
            var res = await _authService.Login(request.Username, request.Password);
            if (res == null) return Unauthorized();

            return Ok(res);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] string request)
        {
            var token = await _authService.RefreshToken(request);
            if (token == null) return Unauthorized();

            return Ok(token);
        }

        [HttpPost("autho")]
        public async Task<IActionResult> Autho(int UserId, [FromBody] List<Permission> roles)
        {
            if (UserId == 0 || UserId.ToString() == "" || UserId == null)
            {
                return BadRequest("User not found");
            }

            if (roles == null || roles.Count == 0)
            {
                return BadRequest("List permission not found");
            }
            foreach(var per in roles)
            {
                var perId = await _userRoleService.CheckPermissionByUserIdAndTable(UserId, per.TableName);
                if(perId != 0)
                {
                    int id = perId;
                    await _permissionService.DeletePermission(id);
                    await _roleService.DeleteRole(id);
                    await _rolePermissionsService.DeleteRolePermission(id);
                    await _userRoleService.DeleteUserRole(id);
                    await _roleMenuAccessService.DeleteRoleMenuAccess(id);
                }
            }
            int affectedRows = 0;
            foreach (var role in roles)
            {             
                var permissionResult = await _permissionService.CreatePermission(role);
                affectedRows += permissionResult;
             
                var tempRole = new Role
                {
                    RoleId = role.PermissionId, 
                    RoleName = "",  
                    Description = "" 
                };
                var roleResult = await _roleService.CreateRole(tempRole);
                affectedRows += roleResult;

                var rolePermissionResult = await _rolePermissionsService.CreateRolePermission(new RolePermission
                {
                    Id = 0,
                    RoleId = tempRole.RoleId,
                    PermissionId = role.PermissionId,
                });
                affectedRows += rolePermissionResult;

                var userRoleResult = await _userRoleService.CreateUserRole(new UserRole
                {
                    Id = 0,
                    UserId = UserId,
                    RoleId = tempRole.RoleId,
                });
                affectedRows += userRoleResult;
              
                var menu = await _menuService.GetMenu(role.TableName);
                var roleMenuAccessResult = await _roleMenuAccessService.CreateRoleMenuAccess(new RoleMenuAcces
                {
                    AccessId = 0,
                    RoleId = tempRole.RoleId,
                    MenuId = menu.MenuId,
                    CanAccess = true,
                });
                affectedRows += roleMenuAccessResult;
            }

            return Ok(new ResponseModel(true, "Thành công", null, affectedRows));
        }

    }
}

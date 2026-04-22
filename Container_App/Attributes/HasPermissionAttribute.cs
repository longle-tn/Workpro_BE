using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Principal;

namespace Container_App.Attributes
{
    public class HasPermissionAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string _resource;
        private readonly string _action;

        public HasPermissionAttribute(string resource, string action)
        {
            _resource = resource;
            _action = action;
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            // 1️⃣ Chưa đăng nhập
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // 2️ Lấy RoleId từ JWT
            var roleId = user.FindFirst("RoleId")?.Value;
            if (string.IsNullOrEmpty(roleId))
            {
                context.Result = new ForbidResult();
                return;
            }

            // 3️ Lấy permission từ cache
            var cache = context.HttpContext.RequestServices.GetService<IMemoryCache>();
            var cacheKey = $"Permission_Role_{roleId}";

            if (!cache.TryGetValue(cacheKey, out List<string> permissions))
            {
                context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
                return;
            }

            // 4️ Check permission
            var requiredPermission = $"{_resource}_{_action}";

            if (!permissions.Contains(requiredPermission))
            {
                context.Result = new ForbidResult();
            }
        }
    }
}

using Container_App.Attributes;
using Container_App.Core.Interface.Permissions;
using Container_App.Core.Interface.RefreshTokens;
using Container_App.Core.Interface.RolePermissions;
using Container_App.Core.Interface.Users;
using Container_App.Core.Model.Permissions;
using Container_App.Core.Model.RefreshTokens;
using Container_App.Core.Model.Users;
using Container_App.Model.RolePermissions;
using Container_App.Model.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.Design;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Container_App.Controllers
{
    [ApiController]
    public class UserController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IUserServices _userServices;
        private readonly IPermissionService _permissionServices;
        private readonly IMemoryCache _memoryCache;
        private readonly IRolePermissionService _rolePermissionService;
        private readonly IRefreshTokenService _refreshTokenService;
        public UserController(IUserServices userServices, IPermissionService permissionService,
            IConfiguration config, IMemoryCache memoryCache, IRolePermissionService rolePermissionService,
            IRefreshTokenService refreshTokenService)
        {
            _userServices = userServices;
            _permissionServices = permissionService;
            _config = config;
            _memoryCache = memoryCache;
            _rolePermissionService = rolePermissionService;
            _refreshTokenService = refreshTokenService;
        }

        [HttpPost]
        [Route("api/login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // 1. Kiểm tra đầu vào
            if (string.IsNullOrEmpty(dto.username) || string.IsNullOrEmpty(dto.password))
            {
                return BadRequest(new { status = false, message = "Username hoặc Password không được để trống" });
            }

            // 2. Xác thực người dùng
            var user = await _userServices.Login(dto.username, dto.password);
            if (user == null)
            {
                return Unauthorized(new { status = false, message = "Tên đăng nhập hoặc mật khẩu không chính xác" });
            }

            // 3. Xử lý Cache Quyền hạn (Phải theo UserId)
            var cacheKey = $"Permission_Role_{user.RoleId}";
            List<Permission> permissions;
            if (!_memoryCache.TryGetValue(cacheKey, out List<string> permissionKeys))
            {
                permissions = (await _permissionServices.GetListPermissionByUser(user.Id)).ToList();

                permissionKeys = permissions
                    .Select(p => $"{p.ResourceName.ToUpper()}_{p.Action.ToUpper()}")
                    .ToList();

                _memoryCache.Set(cacheKey, permissionKeys, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20),
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                });
            }


            // 4. Tạo Token
            var token = GenerateToken(user);
            var refreshToken = GenerateRefreshToken();

            int insertRefreshToken = await _refreshTokenService.InsertRefreshToken(
                new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    ExpireDate = 30
                });
            if(insertRefreshToken != -1)
            {
                Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = DateTime.UtcNow.AddDays(30)
                });
            }

            // 5. Trả về kết quả
            return Ok(new
            {
                status = true,
                Token = token,
                RefreshToken = refreshToken,
                FullName = user.FullName,
                Permissions = permissionKeys,
                message = "Đăng nhập thành công"
            });
        }

        [HttpPost("refresh-token")]
        public IActionResult RefreshToken()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
                return Unauthorized();

            // TODO: Kiểm tra refreshToken trong Database (nếu dùng rotation)
            // Ở đây demo đơn giản, giả sử hợp lệ

            //var newAccessToken = GenerateToken();
            var newRefreshToken = GenerateRefreshToken();
        

            return Ok(new { });
        }

        [HasPermission("User", "insert")]
        [HttpPost]
        [Route("api/insert-user")]
        public async Task<IActionResult> Insert([FromBody] UserProfile u)
        {
            int result = await _userServices.Insert(u);
            if (result == 0)
            {
                return BadRequest();
            }
            if (result != -1)
            {
                return Json(new { status = true, message = "Thêm tài khoản thành công" });
            }
            return Json(new { status = false, message = "Thêm tài khoản thất bại" });
        }

        [HttpPost]
        [Route("api/insert-role-permission")]
        public async Task<IActionResult> InsertRolePermission([FromBody] CreateRolePermissionDto dto)
        {
            var result = await _rolePermissionService.Insert(dto.RoleId, dto.RolePermissions);
            if (result == 0)
            {
                return BadRequest();
            }
            return Ok(result);
        }

        private string GenerateToken(UserProfile user)
        {
            var jwtSection = _config.GetSection("Jwt");
            var jwtKey = jwtSection["Key"];
            var expireMinutes = jwtSection["ExpireMinutes"];

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim("FullName", user.FullName.ToString()),
                new Claim("RoleId", user.RoleId.ToString()),
                new Claim(ClaimTypes.Role, user.RoleName.ToString())
            };

            var key = new SymmetricSecurityKey(Convert.FromBase64String(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(expireMinutes)
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                return Convert.ToBase64String(randomNumber);
            }
        }
    }
}

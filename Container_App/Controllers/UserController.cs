using Container_App.Core.Interface.Permissions;
using Container_App.Core.Interface.Users;
using Container_App.Core.Model.Permissions;
using Container_App.Core.Model.Users;
using Container_App.Model.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        public UserController(IUserServices userServices, IPermissionService permissionService,
            IConfiguration config, IMemoryCache memoryCache)
        {
            _userServices = userServices;
            _permissionServices = permissionService;
            _config = config;
            _memoryCache = memoryCache;
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
            string cacheKey = $"Permission_{user.Id}";
            IEnumerable<Permission> permissions;

            if(!_memoryCache.TryGetValue(cacheKey, out permissions))
            {
                permissions = await _permissionServices.GetListPermissionByUser(user.Id);
                if (permissions != null)
                {
                    var cacheOptions = new MemoryCacheEntryOptions
                    {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(20),
                        SlidingExpiration = TimeSpan.FromMinutes(5) // optional
                    };

                    _memoryCache.Set(cacheKey, permissions, cacheOptions);
                }
            }

            // 4. Tạo Token
            var token = GenerateToken(user);

            // 5. Trả về kết quả
            return Ok(new
            {
                status = true,
                Token = token,
                UserId = user.Id,
                FullName = user.FullName,
                Permissions = permissions,
                message = "Đăng nhập thành công"
            });
        }

        [HttpPost]
        [Route("api/insert-user")]
        [Authorize]
        public async Task<IActionResult> Insert([FromBody] CreateUserDto u)
        {
            var userCreate = new UserProfile()
            {
                UserName = u.UserName,
                Password = u.Password,
                FullName = u.FullName,
                Email = u.Email,
                Address = u.Address,
                CreateBy = u.CreateBy,
            };
            int result = await _userServices.Insert(userCreate);
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

            var claims = new[]
            {
                new Claim("UserId", user.Id.ToString()),
                new Claim("FullName", user.FullName.ToString())
            };

            var key = new SymmetricSecurityKey(Convert.FromBase64String(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: jwtSection["Issuer"],
                audience: jwtSection["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    int.Parse(jwtSection["ExpireMinutes"])
                ),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}

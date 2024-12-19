using Container_App.Model.Tokens;
using Container_App.Model.Users;
using Container_App.Repository.AuthRepository;
using Container_App.Repository.UserRepository;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Container_App.Model.Dto;
using Container_App.Repository.MenuRepository;


namespace Container_App.Services.AuthService
{
    public class AuthService : IAuthService
    {

        private readonly IAuthRepository _authRepository;
        private readonly string _jwtSecretKey;
        private readonly IConfiguration _configuration;
        private readonly IMenuRepository _menuRepository;
        public AuthService(IAuthRepository authRepository, IConfiguration configuration, IMenuRepository menuRepository)
        {
            _authRepository = authRepository;
            _jwtSecretKey = configuration["JWT_SECRET_KEY"];
            _configuration = configuration;
            _menuRepository = menuRepository;
        }

        public Task<List<UserPermission>> GetUserPermissions(int userId)
        {
            return _authRepository.GetUserPermissions(userId);
        }

        public Task<bool> HasPermission(int userId, string table, string action)
        {
            return _authRepository.HasPermission(userId, table, action);
        }

        public async Task<LoginResponseDto> Login(string username, string password)
        {
            var user = await _authRepository.GetUserByUsernameAndPassword(username, password);
            if (user == null) return null;

            var token = GenerateAccessToken(user);
            var menus = await _menuRepository.GetUserMenus(user.UserId);
            var permissions = await _menuRepository.GetUserPermissions(user.UserId);
            return new LoginResponseDto
            {
                AccessToken = token,
                Menus = menus,
                Permissions = permissions
            };
        }

        public async Task<Token> RefreshToken(string refreshToken)
        {
            var storedToken = await _authRepository.GetRefreshToken(refreshToken);

            if (storedToken == null || storedToken.ExpiryDate < DateTime.UtcNow)
            {
                return null; // Invalid or expired refresh token
            }

            var user = await _authRepository.GetUserByID(storedToken.UserId);
            if (user == null) return null;

            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken(user);

            await _authRepository.RevokeRefreshToken(refreshToken); // Revoke old refresh token
            await _authRepository.SaveRefreshToken(newRefreshToken); // Save new refresh token

            return new Token
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken.Token
            };
        }

        private string GenerateAccessToken(Users user)
        {
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.FullName.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Phone.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Email.ToString())
            };

            var jwtSecretKey = _jwtSecretKey;
            if (string.IsNullOrEmpty(jwtSecretKey))
            {
                throw new InvalidOperationException("JWT_SECRET_KEY must be set in the .env file.");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: null,
                audience: null,
                claims: claims,
                expires: DateTime.Now.AddDays(1), // Access token expires in 1 day
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private RefreshTokenModel GenerateRefreshToken(Users user)
        {
            var refreshToken = new RefreshTokenModel
            {
                Token = Guid.NewGuid().ToString(), // Random token string
                ExpiryDate = DateTime.UtcNow.AddDays(7), // Refresh token expires in 7 days
                UserId = user.UserId,
                IsRevoked = false
            };

            return refreshToken;
        }
    }
}

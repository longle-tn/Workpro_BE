using Container_App.Services.UserService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Container_App.utilities;
using Container_App.Model.Users;
using Container_App.Services.AuthService;

namespace Container_App.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public UserController(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        [Authorize]
        [HttpPost("get-all-user")]
        public async Task<ActionResult<List<Users>>> GetUsers(PagedResult page)
        {
            var users = await _userService.GetUsers(page);
            var countRecord = await _userService.CountRecord();
            var response = new ResponseModel(
                        success: true,
                        message: "Get danh sách user thành công!",
                        data: users,
                        affectedRows: countRecord
                    );
            return Ok(response);
        }

        //[Authorize]
        [HttpGet("get-user-by-id/{id}")]
        public async Task<ActionResult<Users>> GetUserById(int id)
        {
            var user = await _userService.GetUserById(id);
            if (user == null)
                return NotFound(new ResponseModel(false, "Không tìm thấy user!"));
            return Ok(user);
        }


        [Authorize]
        [HttpPost("create-user")]
        public async Task<ActionResult<ResponseModel>> CreateUser(Users user)
        {
            var userId = Convert.ToInt32(User.FindFirst("jti")?.Value);
            try
            {
                var hasPermission = await _authService.HasPermission(userId, "Users", "add");

                if (!hasPermission)
                {
                    var response = new ResponseModel(
                        success: false,
                        message: "Bạn không có quyền thêm user!",                       
                        data: null,
                        affectedRows: 0
                    );
                    return Ok(response);
                }
                int rowsAffected = await _userService.CreateUser(user);

                if (rowsAffected > 0)
                {
                    var response = new ResponseModel(
                        success: true,
                        message: "Thêm user thành công!",
                        data: user, // Trả về đối tượng user đã tạo
                        affectedRows: rowsAffected
                    );

                    return rowsAffected > 0 ? Ok(response) : NotFound(response);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new ResponseModel(false, "Thêm user thất bại!"));
            }
            return Ok("Thêm user thất bại!");
        }

        [Authorize]
        [HttpPut("update-user")]
        public async Task<ActionResult<ResponseModel>> UpdateUser([FromBody] Users user)
        {
            var userId = Convert.ToInt32(User.FindFirst("jti")?.Value);
            var hasPermission = await _authService.HasPermission(userId, "Users", "edit");

            if (!hasPermission)
            {
                return Forbid("Bạn không có quyền cập nhật user.");
            }
            int rowsAffected = await _userService.UpdateUser(user);

            var response = new ResponseModel(
                success: rowsAffected > 0,
                message: rowsAffected > 0 ? "Cập nhật user thành công!" : "Không tìm thấy user để cập nhật!",
                data: user,
                affectedRows: rowsAffected
            );

            return rowsAffected > 0 ? Ok(response) : NotFound(response);
        }


        [Authorize]
        [HttpDelete("delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var userId = Convert.ToInt32(User.FindFirst("jti")?.Value);
            var hasPermission = await _authService.HasPermission(userId, "Users", "delete");

            if (!hasPermission)
            {
                return Forbid("Bạn không có quyền xoá user user.");
            }
            int rowsAffected = await _userService.DeleteUser(id);

            var response = new ResponseModel(
                success: rowsAffected > 0,
                message: rowsAffected > 0 ? "Xoá user thành công!" : "Không tìm thấy user để xóa!",
                affectedRows: rowsAffected // Gửi số dòng bị ảnh hưởng
            );

            return rowsAffected > 0 ? Ok(response) : NotFound(response);
        }
    }
}

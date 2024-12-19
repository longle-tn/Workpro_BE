using Container_App.Model.Projects;
using Container_App.Model.ProjectUserInvites;
using Container_App.Model.Users;
using Container_App.Services.AuthService;
using Container_App.Services.ProjectService;
using Container_App.utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Container_App.Controllers
{
    public class ProjectController : ControllerBase
    {
        private readonly IProjectService _projectService;
        private readonly IAuthService _authService;

        public ProjectController(IProjectService projectService, IAuthService authService)
        {
            _projectService = projectService;
            _authService = authService;
        }

        [Authorize] // Yêu cầu xác thực
        [HttpPost("create-project")]
        public async Task<ActionResult<Project>> CreateProject([FromBody] Project project)
        {
            var userId = Convert.ToInt32(User.FindFirst("jti")?.Value);
          

            try
            {
                // Kiểm tra quyền của người dùng
                var hasPermission = await _authService.HasPermission(userId, "Project", "add");

                if (!hasPermission)
                {
                    return Forbid("Bạn không có quyền thêm dự án.");
                }

                // Nếu có quyền, tiếp tục tạo project
                var createdProject = await _projectService.CreateProjectAsync(project, userId);

                var response = new ResponseModel(
                    success: true,
                    message: "Thêm project thành công!",
                    data: createdProject
                );
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message); // Trả về lỗi 403 nếu không đủ quyền
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); // Trả về lỗi 500 cho các lỗi khác
            }
        }

        [Authorize]
        [HttpPost("islock-project")]
        public async Task<ActionResult<Project>> IsLockProject(int projectId)
        {
            try
            {
                bool pro = await _projectService.IsLockProjectAsync(projectId);
                return Ok(pro);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); // Trả về lỗi 500 cho các lỗi khác
            }
        }

        [Authorize]
        [HttpPost("send-invites")]
        public async Task<IActionResult> SendProjectInvites([FromBody] InviteRequest request)
        {
            if (request.UserIds == null || request.UserIds.Count == 0)
                return BadRequest("User IDs are required.");

            int result = await _projectService.SendProjectInvitesAsync(request.ProjectId, request.UserIds);
            var response = new ResponseModel(
                    success: true,
                    message: "Gửi lời mời thành công!",
                    data: result
                );
            return Ok(response);
        }

        [Authorize]
        [HttpPost("accept-invite/{inviteId}")]
        public async Task<IActionResult> AcceptInvite(int inviteId)
        {
            var result = await _projectService.AcceptInviteAsync(inviteId);
            if (!result)
                return NotFound("Invite not found or already accepted.");

            var response = new ResponseModel(
                    success: true,
                    message: "Chấp nhận lời mời thành công!",
                    data: result
                );
            return Ok(response);
        }

        [Authorize] 
        [HttpPost("decline-invite/{inviteId}")]
        public async Task<IActionResult> DeclineInvite(int inviteId)
        {
            var result = await _projectService.DeclineInviteAsync(inviteId);
            if (!result)
                return NotFound("Invite not found or already declined.");

            var response = new ResponseModel(
                    success: true,
                    message: "Từ chối lời mời thành công!",
                    data: result
                );
            return Ok(response);
        }
    }
}

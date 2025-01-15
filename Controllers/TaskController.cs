using Container_App.Model.Projects;
using Container_App.Model.Tasks;
using Container_App.Services.AuthService;
using Container_App.Services.ProjectService;
using Container_App.Services.TaskService;
using Container_App.utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Container_App.Controllers
{
    public class TaskController : ControllerBase
    {
        private readonly ITaskService _taskService;
        private readonly IAuthService _authService;

        public TaskController(ITaskService taskService, IAuthService authService)
        {
            _taskService = taskService;
            _authService = authService;
        }

        [Authorize] // Yêu cầu xác thực
        [HttpPost("create-task")]
        public async Task<ActionResult<Project>> CreateTask([FromBody] Tasks task)
        {
            var userId = Convert.ToInt32(User.FindFirst("jti")?.Value);

            try
            {
                // Kiểm tra quyền của người dùng
                var hasPermission = await _authService.HasPermission(userId, "Task", "add");

                if (!hasPermission)
                {
                    return Forbid("Bạn không có quyền thêm task.");
                }

                // Nếu có quyền, tiếp tục tạo project
                var createdTask = await _taskService.AddTaskAsync(task, userId);

                var response = new ResponseModel(
                    success: true,
                    message: "Thêm task thành công!",
                    data: createdTask
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

        [Authorize] // Yêu cầu xác thực
        [HttpPost("complete-task")]
        public async Task<ActionResult<Project>> CompleteTask([FromBody] int taskId)
        {
            var userId = Convert.ToInt32(User.FindFirst("jti")?.Value);


            try
            {            
                // Nếu có quyền, tiếp tục tạo project
                var complete = await _taskService.CompleteTask(taskId, userId);

                var response = new ResponseModel(
                    success: true,
                    message: "Complete task thành công!",
                    data: complete
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
    }
}

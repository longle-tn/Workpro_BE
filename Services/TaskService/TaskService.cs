using Container_App.Model.Tasks;
using Container_App.Repository.TaskRepository;

namespace Container_App.Services.TaskService
{
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        public TaskService(ITaskRepository taskRepository)
        {
            _taskRepository = taskRepository;
        }
        public async Task<int> AddTaskAsync(Tasks task, int userId)
        {
            return await _taskRepository.AddTaskAsync(task, userId);
        }

        public async Task<int> CompleteTask(int taskId, int userId)
        {
            return await _taskRepository.CompleteTask(taskId, userId);
        }
    }
}

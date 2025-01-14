using Container_App.Model.Tasks;

namespace Container_App.Services.TaskService
{
    public interface ITaskService
    {
        Task<int> AddTaskAsync(Tasks task, int userId);
    }
}

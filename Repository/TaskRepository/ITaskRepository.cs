using Container_App.Model.Projects;
using Container_App.Model.Tasks;

namespace Container_App.Repository.TaskRepository
{
    public interface ITaskRepository
    {
        Task<int> AddTaskAsync(Tasks task, int userId);
    }
}

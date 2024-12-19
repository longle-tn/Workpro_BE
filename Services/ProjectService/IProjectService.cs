using Container_App.Model.Projects;

namespace Container_App.Services.ProjectService
{
    public interface IProjectService
    {
        Task<int> CreateProjectAsync(Project project, int userId);
        Task<bool> IsLockProjectAsync(int projectId);
        Task <int>SendProjectInvitesAsync(int projectId, List<int> userIds);
        Task<bool> AcceptInviteAsync(int inviteId);
        Task<bool> DeclineInviteAsync(int inviteId);
    }
}

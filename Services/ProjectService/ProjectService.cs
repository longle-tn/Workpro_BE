using Container_App.Data;
using Container_App.Model.Projects;
using Container_App.Model.ProjectUsers;
using Container_App.Repository.ProjectRepository;
using Container_App.Repository.ProjectUserInviteRepository;
using Container_App.Repository.ProjectUserRepository;
using Container_App.Repository.UserRepository;
using Microsoft.EntityFrameworkCore;

namespace Container_App.Services.ProjectService
{
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;      
        private readonly IUserRepository _userRepository;
        private readonly IProjectUserRepository _projectuserRepository;
        private readonly IProjectUserInviteRepository _projectuserInviteRepository;

        public ProjectService(IProjectRepository projectRepository, IUserRepository userRepository, IProjectUserInviteRepository projectUserInviteRepository,
            IProjectUserRepository projectUserRepository)
        {
            _projectRepository = projectRepository;         
            _userRepository = userRepository;
            _projectuserInviteRepository = projectUserInviteRepository;
            _projectuserRepository = projectUserRepository;
        }

        public async Task<bool> AcceptInviteAsync(int inviteId)
        {
            var invite = await _projectuserInviteRepository.GetInviteAsync(inviteId);
            if (invite == null || invite.Status == 0) // Ensure invite is pending
                return false;

            await _projectuserInviteRepository.AcceptInviteAsync(invite);

            var projectUser = new ProjectUser
            {
                ProjectId = invite.ProjectId,
                UserId = invite.UserId,
                Role = "Member",
                JoinedAt = DateTime.Now
            };

            await _projectuserRepository.AddUserToProjectAsync(projectUser);
            return true;
        }

        public async Task<int> CreateProjectAsync(Project project, int userId)
        {         
            project.CreatedBy = userId;
            return await _projectRepository.AddProjectAsync(project, userId);
        }

        public async Task<bool> DeclineInviteAsync(int inviteId)
        {
            var invite = await _projectuserInviteRepository.GetInviteAsync(inviteId);
            if (invite == null || invite.Status != 0)
                return false;

            await _projectuserInviteRepository.DeclineInviteAsync(invite);
            return true;
        }

        public Task<bool> IsLockProjectAsync(int projectId)
        {
            return _projectRepository.IsLockProjectAsync(projectId);
        }

        public async Task<int> SendProjectInvitesAsync(int projectId, List<int> userIds)
        {
            return await _projectuserInviteRepository.SendInvitesAsync(projectId, userIds);
        }
    }
}

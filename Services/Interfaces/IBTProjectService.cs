using BugTrackerMVC.Models;
using System.ComponentModel.Design;

namespace BugTrackerMVC.Services.Interfaces
{
    // actions for Project
    public interface IBTProjectService
    {
        // get all projects for specific company
        public Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId);

        // get archived projects for specific company
        public Task<List<Project>> GetArchivedProjectByCompanyIdAsync(int companyId);

        // get user projects
        public Task<List<Project>?> GetUserProjectsAsync(string userId);

        // add project to db
        public Task AddProjectAsync(Project project);

        // get project by id
        public Task<Project> GetProjectByIdAsync(int projectId, int companyId);

        // update project to db
        public Task UpdateProjectAsync(Project project);

        // archive project
        public Task ArchiveProjectAsync(Project project);
        
        // restore project
        public Task RestoreProjectAsync(Project project);

        // get project priority list
        public Task<IEnumerable<ProjectPriority>> GetProjectPrioritiesAsync();

        // get project manager
        public Task<BTUser> GetProjectManagerAsync(int projectId);

        // get developer
        public Task<BTUser> GetDeveloperAsync(int projectId);

        // get submitter
        public Task<BTUser> GetSubmitterAsync(int projectId);

        // add project manager
        public Task<bool> AddProjectManagerAsync(string userId, int projectId);

        // remove project manager
        public Task RemoveProjectManagerAsync(int projectId);

        // add Member to project
        public Task<bool> AddMemberToProjectAsync(BTUser member, int projectId);

        // remove Member from project
        public Task<bool> RemoveMemberFromProjectAsync(BTUser member, int projectId);

        // get projects by priority
        public Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priority);

        // get projects members by role
        public Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string roleName);
    }
}

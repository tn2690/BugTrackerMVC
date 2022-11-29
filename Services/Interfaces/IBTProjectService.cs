using BugTrackerMVC.Models;

namespace BugTrackerMVC.Services.Interfaces
{
    // actions for Project
    public interface IBTProjectService
    {
        // get all projects for specific company
        public Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId);

        // get archived projects for specific company
        public Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId);

        // get details for a project
        //public Task<Project> GetProjectDetailsByIdAsync(int? id);

        // add project to db
        public Task AddProjectAsync(Project project);

        // get project by id
        public Task<Project> GetProjectByIdAsync(int? projectId, int companyId);

        // update project to db
        public Task UpdateProjectAsync(Project project);

        //// archive project
        //public Task ArchiveProjectAsync(Project project);

        //// restore project
        //public Task RestoreProjectAsync(Project project);

        // get project priority list
        public Task<IEnumerable<ProjectPriority>> GetProjectPrioritiesAsync();

        // add project manager
        public Task<bool> AddProjectManagerAsync(string userId, int projectId);

        // remove project manager
        public Task RemoveProjectManagerAsync(int projectId);

        // add Member to project
        public Task<bool> AddMemberToProjectAsync(BTUser member, int projectId);

        // remove Member from project
        public Task<bool> RemoveMemberFromProjectAsync(BTUser member, int projectId);


        // get projects for specific user
        //public Task<List<Project>> GetProjectsUserAsync(int userId);

        

        // get company for specific user
        //public Task<List<Company>> GetCompanyAsync(int companyId);


    }
}

using BugTrackerMVC.Models;

namespace BugTrackerMVC.Services.Interfaces
{
    // actions for bug tracker
    public interface IBugTrackerService
    {
        // get projects for specific company
        public Task<List<Project>> GetProjectsAsync(int companyId);

        // get projects for specific user
        //public Task<List<Project>> GetProjectsUserAsync(int userId);

        // get archived projects
        public Task<List<Project>> GetArchivedProjectsAsync(int companyId);

        // get company for specific user
        public Task<List<Company>> GetCompanyAsync(int companyId);

        
    }
}

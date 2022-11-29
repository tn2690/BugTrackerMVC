using BugTrackerMVC.Models;

namespace BugTrackerMVC.Services.Interfaces
{
    public interface IBTTicketService
    {
        // get all projects for specific company
        //public Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId);

        // get archived projects for specific company
        //public Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId);

        // get details for a project
        //public Task<Project> GetProjectDetailsByIdAsync(int? id);

        // add ticket to db
        public Task AddTicketAsync(Ticket ticket);

        // get project by id
        //public Task<Project> GetProjectByIdAsync(int? projectId);

        // update ticket to db
        public Task UpdateTicketAsync(Ticket ticket);

        // archive ticket
        public Task ArchiveTicketAsync(Ticket ticket);

        // restore ticket
        public Task RestoreTicketAsync(Ticket ticket);

        // get ticket priority list
        public Task<List<TicketPriority>> GetTicketPrioritiesAsync();
    }
}

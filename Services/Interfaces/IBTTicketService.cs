using BugTrackerMVC.Models;

namespace BugTrackerMVC.Services.Interfaces
{
    public interface IBTTicketService
    {
        // get all tickets for specific company
        public Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int companyId);

        // add ticket to db
        public Task AddTicketAsync(Ticket ticket);

        // get ticket by id
        public Task<Ticket> GetTicketByIdAsync(int projectId);

        // update ticket to db
        public Task UpdateTicketAsync(Ticket ticket);

        // get ticket priority list
        public Task<IEnumerable<TicketPriority>> GetTicketPrioritiesAsync();
        
        // get ticket status list
        public Task<IEnumerable<TicketStatus>> GetTicketStatusesAsync();
        
        // get ticket type list
        public Task<IEnumerable<TicketType>> GetTicketTypesAsync();

        // get list of projects
        public Task<IEnumerable<Project>> GetProjectsAsync();

        // get list of users
        public Task<List<BTUser>> GetUsersAsync();

        // get developer
        //public async Task<BTUser> GetDeveloperAsync(int ticketId);

        // assign a Developer to ticket
        //public Task<bool> AssignDeveloperAsync(string userId, BTUser member, int ticketId);

        // remove a Developer from ticket
        //public Task RemoveDeveloperAsync(BTUser member, int ticketId);
    }
}

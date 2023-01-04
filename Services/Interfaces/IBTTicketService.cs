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
        public Task<Ticket> GetTicketByIdAsync(int projectId, int companyId);

        // update ticket to db
        public Task UpdateTicketAsync(Ticket ticket);

        // archive ticket
        public Task ArchiveTicketAsync(Ticket ticket);

        // restore ticket
        public Task RestoreTicketAsync(Ticket ticket);

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

        // assign a Developer to ticket
        public Task AssignDeveloperAsync(int ticketId, string userId, int companyId);

        // add file attachments
        public Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment);

        // download attachments
        public Task<TicketAttachment> GetTicketAttachmentsByIdAsync(int ticketAttachmentId);

        // add comment
        public Task AddCommentAsync(TicketComment ticketComment);

        // get ticket by user id
        public Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId);

        // get ticket with no tracking history
        public Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId, int companyId);
    }
}

using BugTrackerMVC.Data;
using BugTrackerMVC.Models;
using BugTrackerMVC.Services.Interfaces;

namespace BugTrackerMVC.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;

        public BTTicketService(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task AddTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }
        public Task UpdateTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }

        public Task ArchiveTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }

        public Task RestoreTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }

        public Task<List<TicketPriority>> GetTicketPrioritiesAsync()
        {
            throw new NotImplementedException();
        }

        
    }
}

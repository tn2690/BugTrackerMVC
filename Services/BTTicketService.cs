using BugTrackerMVC.Data;
using BugTrackerMVC.Enums;
using BugTrackerMVC.Models;
using BugTrackerMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTrackerMVC.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;

        public BTTicketService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = await _context.Tickets
                                                     .Where(t => t.Project!.CompanyId == companyId)
                                                     .Include(t => t.DeveloperUser)
                                                     .Include(t => t.Project)
                                                        .ThenInclude(t => t.Company)
                                                     .Include(t => t.SubmitterUser)
                                                     .Include(t => t.TicketPriority)
                                                     .Include(t => t.TicketStatus)
                                                     .Include(t => t.TicketType)
                                                     .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task AddTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets
                                               .Include(t => t.DeveloperUser)
                                               .Include(t => t.Project)
                                               .Include(t => t.SubmitterUser)
                                               .Include(t => t.TicketPriority)
                                               .Include(t => t.TicketStatus)
                                               .Include(t => t.TicketType)
                                               .FirstOrDefaultAsync(t => t.Id == ticketId);

                return ticket!;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<TicketPriority>> GetTicketPrioritiesAsync()
        {
            try
            {
                IEnumerable<TicketPriority> ticketPriorities = await _context.TicketPriorities
                                                                        .ToListAsync();

                return ticketPriorities;

            } catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<TicketStatus>> GetTicketStatusesAsync()
        {
            try
            {
                IEnumerable<TicketStatus> ticketStatus = await _context.TicketStatuses
                                                                        .ToListAsync();

                return ticketStatus;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<TicketType>> GetTicketTypesAsync()
        {
            try
            {
                IEnumerable<TicketType> ticketTypes = await _context.TicketTypes
                                                                        .ToListAsync();

                return ticketTypes;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<Project>> GetProjectsAsync()
        {
            try
            {
                IEnumerable<Project> projects = await _context.Projects
                                                              .ToListAsync();

                return projects;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<BTUser>> GetUsersAsync()
        {
            try
            {
                List<BTUser> users = await _context.Users
                                             .ToListAsync();

                return users;

            } catch (Exception)
            {
                throw;
            }
        }
    }
}

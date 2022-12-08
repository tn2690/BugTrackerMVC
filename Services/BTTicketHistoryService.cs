using BugTrackerMVC.Data;
using BugTrackerMVC.Models;
using BugTrackerMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTrackerMVC.Services
{
    public class BTTicketHistoryService : IBTTicketHistoryService
    {
        private readonly ApplicationDbContext _context;

        public BTTicketHistoryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddHistoryAsync(Ticket oldTicket, Ticket newTicket, string userId)
        {
            // new ticket has been added
            if (oldTicket == null && newTicket != null)
            {
                TicketHistory history = new()
                {
                    TicketId = newTicket.Id,
                    PropertyName = "",
                    OldValue = "",
                    NewValue = "",
                    Created = DateTime.UtcNow,
                    BTUserId = userId,
                    Description = "New Ticket Created"
                };

                try
                {
                    await _context.TicketHistories.AddAsync(history);
                    await _context.SaveChangesAsync();
                }
                catch (Exception)
                {

                    throw;
                }
            }
            else
            {
                // check ticket title
                if (oldTicket!.Title != newTicket!.Title)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        PropertyName = "Title",
                        OldValue = oldTicket.Title,
                        NewValue = newTicket.Title,
                        Created = DateTime.UtcNow,
                        BTUserId = userId,
                        Description = $"New ticket title: {newTicket.Title}"
                    };
                    await _context.TicketHistories.AddAsync(history);
                }

                // check ticket description
                if (oldTicket.Description != newTicket.Description)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        PropertyName = "Description",
                        OldValue = oldTicket.Description,
                        NewValue = newTicket.Description,
                        Created = DateTime.UtcNow,
                        BTUserId = userId,
                        Description = $"New ticket description: {newTicket.Description}"
                    };
                    await _context.TicketHistories.AddAsync(history);
                }

                // check ticket priority
                if (oldTicket.TicketPriorityId != newTicket.TicketPriorityId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        PropertyName = "TicketPriority",
                        OldValue = oldTicket.TicketPriority!.Name,
                        NewValue = newTicket.TicketPriority!.Name,
                        Created = DateTime.UtcNow,
                        BTUserId = userId,
                        Description = $"New ticket priority: {newTicket.TicketPriority.Name}"
                    };
                    await _context.TicketHistories.AddAsync(history);
                }

                // check ticket status
                if (oldTicket.TicketStatusId != newTicket.TicketStatusId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        PropertyName = "TicketStatus",
                        OldValue = oldTicket.TicketStatus!.Name,
                        NewValue = newTicket.TicketStatus!.Name,
                        Created = DateTime.UtcNow,
                        BTUserId = userId,
                        Description = $"New ticket Status: {newTicket.TicketStatus.Name}"
                    };
                    await _context.TicketHistories.AddAsync(history);
                }

                // check ticket type
                if (oldTicket.TicketTypeId != newTicket.TicketTypeId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        PropertyName = "TicketTypeId",
                        OldValue = oldTicket.TicketType!.Name,
                        NewValue = newTicket.TicketType!.Name,
                        Created = DateTime.UtcNow,
                        BTUserId = userId,
                        Description = $"New ticket Type: {newTicket.TicketType.Name}"
                    };
                    await _context.TicketHistories.AddAsync(history);
                }

                // check ticket developer
                if (oldTicket.DeveloperUserId != newTicket.DeveloperUserId)
                {
                    TicketHistory history = new()
                    {
                        TicketId = newTicket.Id,
                        PropertyName = "Developer",
                        OldValue = oldTicket.DeveloperUser?.FullName ?? "Not Assigned",
                        NewValue = newTicket.DeveloperUser?.FullName,
                        Created = DateTime.UtcNow,
                        BTUserId = userId,
                        Description = $"New ticket developer: {newTicket.DeveloperUser!.FullName}"

                    };
                    await _context.TicketHistories.AddAsync(history);
                }

                try
                {
                    // save to db
                    await _context.SaveChangesAsync();

                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public async Task AddHistoryAsync(int ticketId, string model, string userId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets.FindAsync(ticketId);
                string description = model.ToLower().Replace("ticket", "");
                description = $"New {description} added to ticket: {ticket!.Title}";


                TicketHistory history = new()
                {
                    TicketId = ticket.Id,
                    PropertyName = model,
                    OldValue = "",
                    NewValue = "",
                    Created = DateTime.UtcNow,
                    BTUserId = userId,
                    Description = description
                };

                await _context.TicketHistories.AddAsync(history);
                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<TicketHistory>> GetProjectTicketsHistoriesAsync(int projectId, int companyId)
        {
            try
            {
                Project? project = await _context.Projects.Where(p => p.CompanyId == companyId)
                                                          .Include(p => p.Tickets)
                                                                .ThenInclude(t => t.History)
                                                                .ThenInclude(h => h.BTUser)
                                                          .FirstOrDefaultAsync(p => p.Id == projectId);

                List<TicketHistory> ticketHistory = project!.Tickets.SelectMany(t => t.History).ToList();

                return ticketHistory;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<TicketHistory>> GetCompanyTicketsHistoriesAsync(int companyId)
        {
            try
            {
                List<Project> projects = (await _context.Companies
                                                        .Include(c => c.Projects)
                                                                .ThenInclude(p => p.Tickets)
                                                                .ThenInclude(t => t.History)
                                                                    .ThenInclude(h => h.BTUser)
                                                        .FirstOrDefaultAsync(c => c.Id == companyId))!.Projects.ToList();

                List<Ticket> tickets = projects.SelectMany(p => p.Tickets).ToList();

                List<TicketHistory> ticketHistories = tickets.SelectMany(t => t.History).ToList();

                return ticketHistories;

            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

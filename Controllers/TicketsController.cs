using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTrackerMVC.Data;
using BugTrackerMVC.Models;
using Microsoft.AspNetCore.Identity;
using BugTrackerMVC.Enums;
using BugTrackerMVC.Helpers;
using BugTrackerMVC.Services.Interfaces;
using BugTrackerMVC.Extensions;

namespace BugTrackerMVC.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _btTicketService;

        public TicketsController(ApplicationDbContext context,
                                UserManager<BTUser> userManager,
                                IBTTicketService btTicketService)
        {
            _context = context;
            _userManager = userManager;
            _btTicketService = btTicketService;
        }

        // GET: Tickets/AllTickets
        public async Task<IActionResult> AllTickets()
        {
            int companyId = User.Identity!.GetCompanyId();

            // call service
            List<Ticket> tickets = (await _btTicketService.GetAllTicketsByCompanyIdAsync(companyId)).ToList();
            
            return View(tickets);
        }

        // GET: Tickets/ArchivedTickets
        public async Task<IActionResult> ArchivedTickets()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Ticket> tickets = (await _btTicketService.GetAllTicketsByCompanyIdAsync(companyId)).Where(t => t.Archived == true).ToList();

            return View(tickets);
        }

        // GET: Tickets/UnassignedTickets
        public async Task<IActionResult> UnassignedTickets()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Ticket> tickets = (await _btTicketService.GetAllTicketsByCompanyIdAsync(companyId)).ToList();

            return View(tickets);
        }

        // get submitted tickets
        // get the tickets by the user logged in
        // GET: Tickets/MyTickets
        public async Task<IActionResult> MyTickets()
        {
            int companyId = User.Identity!.GetCompanyId();

            // call service
            List<Ticket> tickets = (await _btTicketService.GetAllTicketsByCompanyIdAsync(companyId)).ToList();

            return View(tickets);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // call service
            Ticket? ticket = await _btTicketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["ProjectId"] = new SelectList(await _btTicketService.GetProjectsAsync(), "Id", "Name");
            //ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["TicketPriorityId"] = new SelectList(await _btTicketService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(await _btTicketService.GetTicketStatusesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _btTicketService.GetTicketTypesAsync(), "Id", "Name");

            return View(new Ticket());
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Updated,ProjectId,TicketTypeId,TicketPriorityId")] Ticket ticket)
        {
            ModelState.Remove("SubmitterUserId");

            if (ModelState.IsValid)
            {
                // ticket is initially unassigned when created
                ticket.TicketStatusId = (await _context.TicketStatuses.FirstOrDefaultAsync(s => s.Name == nameof(BTTicketStatuses.New)))!.Id;

                // set submitter user id
                ticket.SubmitterUserId = _userManager.GetUserId(User);

                // set date created, date updated
                ticket.Created = DateTime.UtcNow;

                ticket.Updated = PostgresDate.Format(DateTime.Now);

                // call service
                await _btTicketService.AddTicketAsync(ticket);

                return RedirectToAction(nameof(AllTickets));
            }

            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(await _btTicketService.GetProjectsAsync(), "Id", "Name", ticket.ProjectId);
            ViewData["SubmitterUserId"] = new SelectList(await _btTicketService.GetUsersAsync(), "Id", "Id", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(await _btTicketService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _btTicketService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _btTicketService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);
            
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }


            // call service
            Ticket? ticket = await _btTicketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(await _btTicketService.GetProjectsAsync(), "Id", "Name", ticket.ProjectId);
            ViewData["SubmitterUserId"] = new SelectList(await _btTicketService.GetUsersAsync(), "Id", "FullName", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(await _btTicketService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _btTicketService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _btTicketService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);
            
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // set date created, date updated
                    ticket.Created = DateTime.SpecifyKind(ticket.Created, DateTimeKind.Utc);

                    if (ticket.Updated != null)
                    {
                        ticket.Updated = DateTime.SpecifyKind(ticket.Updated.Value, DateTimeKind.Utc);
                    }

                    // call service
                    await _btTicketService.UpdateTicketAsync(ticket);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(AllTickets));
            }

            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(await _btTicketService.GetProjectsAsync(), "Id", "Name", ticket.ProjectId);
            //ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(await _btTicketService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _btTicketService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _btTicketService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);
            
            return View(ticket);
        }

        // GET: Tickets/Archive/5
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // TODO: call service
            Ticket? ticket = await _btTicketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Restore/5
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // TODO: call service
            Ticket? ticket = await _btTicketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (id == 0)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }

            Ticket? ticket = await _btTicketService.GetTicketByIdAsync(id);

            if (ticket != null)
            {
                // set Archived property to true
                ticket.Archived = true;

                // send ticket to service for update
                await _btTicketService.UpdateTicketAsync(ticket);
            }

            return RedirectToAction(nameof(AllTickets));
        }

        // POST: Tickets/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            if (id == 0)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }

            Ticket? ticket = await _btTicketService.GetTicketByIdAsync(id);

            if (ticket != null)
            {
                // set Archived property to false
                ticket.Archived = false;

                // send ticket to service for update
                await _btTicketService.UpdateTicketAsync(ticket);
            }

            return RedirectToAction(nameof(ArchivedTickets));
        }

        private async Task<bool> TicketExists(int id)
        {
            int companyId = User.Identity!.GetCompanyId();

            return (await _btTicketService.GetAllTicketsByCompanyIdAsync(companyId)).Any(e => e.Id == id);
        }
    }
}

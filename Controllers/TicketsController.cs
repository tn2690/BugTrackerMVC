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
using BugTrackerMVC.Models.Enums;
using BugTrackerMVC.Helpers;
using BugTrackerMVC.Services.Interfaces;
using BugTrackerMVC.Extensions;
using BugTrackerMVC.Models.ViewModels;
using BugTrackerMVC.Services;
using System.ComponentModel.Design;

namespace BugTrackerMVC.Controllers
{
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _btTicketService;
        private readonly IBTRolesService _btRolesService;
        private readonly IBTFileService _btFileService;
        private readonly IBTTicketHistoryService _btTicketHistoryService;
        private readonly IBTNotificationService _btNotificationService;
        private readonly IBTProjectService _btProjectService;

        public TicketsController(ApplicationDbContext context,
                                UserManager<BTUser> userManager,
                                IBTTicketService btTicketService,
                                IBTRolesService btRolesService,
                                IBTFileService btFileService,
                                IBTTicketHistoryService btTicketHistoryService,
                                IBTNotificationService btNotificationService,
                                IBTProjectService btProjectService)
        {
            _context = context;
            _userManager = userManager;
            _btTicketService = btTicketService;
            _btRolesService = btRolesService;
            _btFileService = btFileService;
            _btTicketHistoryService = btTicketHistoryService;
            _btNotificationService = btNotificationService;
            _btProjectService = btProjectService;
        }

        // GET: Tickets/AllTickets
        public async Task<IActionResult> AllTickets(string? swalMessage = null)
        {
            // sweet alert message
            ViewData["SwalMessage"] = swalMessage;

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

            List<Ticket> tickets = (await _btTicketService.GetAllTicketsByCompanyIdAsync(companyId)).Where(t => t.DeveloperUserId == null).ToList();

            return View(tickets);
        }

        // get submitted tickets
        // get the tickets by the user logged in
        // GET: Tickets/MyTickets
        public async Task<IActionResult> MyTickets()
        {
            int companyId = User.Identity!.GetCompanyId();

            // get user logged in
            string userId =_userManager.GetUserId(User);

            // call service
            List<Ticket> tickets = (await _btTicketService.GetAllTicketsByCompanyIdAsync(companyId)).Where(t => t.DeveloperUserId == userId).ToList();

            return View(tickets);
        }

        // GET: Tickets/AssignDeveloper/5
        [HttpGet]
        public async Task<IActionResult> AssignDeveloper(int? id)
        {
            // validate id
            if (id == null)
            {
                return NotFound();
            }

            Ticket? ticket = await _btTicketService.GetTicketByIdAsync(id.Value);

            int companyId = User.Identity!.GetCompanyId();

            List<BTUser> developers = await _btRolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);

            // create/instantiate DevViewModel
            // get and assign Ticket property of view model
            AssignDevViewModel viewModel = new()
            {
                Ticket = ticket,
                DevList = new SelectList(developers, "Id", "FullName", ticket.DeveloperUserId), // create SelectList of company's Devs (highlight current Dev if one is assigned)
                DevId = ticket.DeveloperUserId
            };

            // return View() using AssignPMViewModel
            return View(viewModel);
        }

        // POST: Tickets/AssignDeveloper/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDeveloper(AssignDevViewModel viewModel)
        {
            if (viewModel.Ticket?.Id != null)
            {
                // validate Dev id of viewModel
                if (!string.IsNullOrEmpty(viewModel.DevId))
                {
                    // call service to add Dev
                    await _btTicketService.AssignDeveloperAsync(viewModel.Ticket.Id, viewModel.DevId);
                }
            }

            // add ticket notification
            BTUser btUser = await _userManager.GetUserAsync(User);

            Notification notification = new()
            {
                NotificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(n => n.Name == nameof(BTNotificationTypes.Ticket)))!.Id,
                TicketId = viewModel.Ticket!.Id,
                Title = "Ticket Assignment",
                Message = $"Ticket : {viewModel.Ticket.Title}, was assigned by {btUser.FullName}",
                Created = SetDate.Format(DateTime.Now),
                SenderId = btUser.Id,
                RecipientId = viewModel.DevId
            };

            // sweet alert
            string? swalMessage = string.Empty;

            // add and send notification
            await _btNotificationService.AddNotificationAsync(notification);
            await _btNotificationService.SendEmailNotificationAsync(notification, "Ticket Assignment");

            swalMessage = "Success: Email Sent to Developer!";

            return RedirectToAction("AllTickets", "Tickets", new { swalMessage });
        }

        // POST: Tickets/AddComment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment([Bind("Id,Comment,TicketId")] TicketComment ticketComment)
        {
            // remove BT user id
            ModelState.Remove("BtUserId");

            if (ModelState.IsValid)
            {
                // assign author of comment to BTUserId
                ticketComment.BTUserId = _userManager.GetUserId(User);

                // set date creation of comment
                ticketComment.Created = DateTime.UtcNow;

                await _btTicketService.AddCommentAsync(ticketComment);
            }

            return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId });
        }

        // POST: Tickets/AddTicketAttachment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            // remove BTUserId
            ModelState.Remove("BTUserId");

            string statusMessage;

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                ticketAttachment.FileData = await _btFileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                ticketAttachment.FileType = ticketAttachment.FormFile.ContentType;

                ticketAttachment.Created = DateTime.UtcNow;
                ticketAttachment.BTUserId = _userManager.GetUserId(User);

                await _btTicketService.AddTicketAttachmentAsync(ticketAttachment);
                statusMessage = "Success! New Attachment added to Ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";
            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }

        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _btTicketService.GetTicketAttachmentsByIdAsync(id);
            string fileName = ticketAttachment.FileName!;
            byte[] fileData = ticketAttachment.FileData!;
            string ext = Path.GetExtension(fileName!).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData!, $"application/{ext}");
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
            BTUser btUser = await _userManager.GetUserAsync(User);

            ModelState.Remove("SubmitterUserId");

            if (ModelState.IsValid)
            {
                string userId = _userManager.GetUserId(User);

                // ticket is initially unassigned when created
                ticket.TicketStatusId = (await _context.TicketStatuses.FirstOrDefaultAsync(s => s.Name == nameof(BTTicketStatuses.New)))!.Id;

                // set submitter user id
                ticket.SubmitterUserId = _userManager.GetUserId(User);

                // set date created, date updated
                ticket.Created = DateTime.UtcNow;

                ticket.Updated = SetDate.Format(DateTime.Now);

                // call service
                await _btTicketService.AddTicketAsync(ticket);

                int companyId = User.Identity!.GetCompanyId();

                // add ticket history record
                Ticket newTicket = await _btTicketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                await _btTicketHistoryService.AddHistoryAsync(null!, newTicket, userId);

                // ticket notification
                BTUser? projectManager = await _btProjectService.GetProjectManagerAsync(ticket.ProjectId);

                // sweet alert
                string? swalMessage = string.Empty;

                Notification notification = new()
                {
                    NotificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(n => n.Name == nameof(BTNotificationTypes.Ticket)))!.Id,
                    TicketId = ticket.Id,
                    Title = "New Ticket Added",
                    Message = $"New Ticket : {ticket.Title}, was assigned by {btUser.FullName}",
                    Created = SetDate.Format(DateTime.Now),
                    SenderId = userId,
                    RecipientId = projectManager?.Id
                };

                // test if PM is null
                if (projectManager != null)
                {
                    await _btNotificationService.AddNotificationAsync(notification);
                    await _btNotificationService.SendEmailNotificationAsync(notification, $"New Ticket Added for Project: {ticket.Project!.Name}");

                    swalMessage = "Success: Email Sent to Recipients!";

                    return RedirectToAction("AllTickets", "Tickets", new { swalMessage });
                }
                else
                {
                    // send email to admins
                    await _btNotificationService.AdminNotificationAsync(notification, companyId);
                    await _btNotificationService.SendAdminEmailNotificationAsync(notification, $"New Ticket Added for Project: {ticket.Project!.Name}", companyId);

                    swalMessage = "Success: Email Sent to Admins!";

                    return RedirectToAction("AllTickets", "Tickets", new { swalMessage });
                }

                //return RedirectToAction(nameof(AllTickets));
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
                // get company id
                int companyId = User.Identity!.GetCompanyId();

                // get logged in user id
                string userId = _userManager.GetUserId(User);

                Ticket? oldTicket = await _btTicketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                try
                {
                    // set date created, date updated
                    ticket.Created = DateTime.SpecifyKind(ticket.Created, DateTimeKind.Utc);

                    ticket.Updated = DateTime.SpecifyKind(ticket.Updated!.Value, DateTimeKind.Utc);

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

                // add history
                Ticket? newTicket = await _btTicketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                await _btTicketHistoryService.AddHistoryAsync(oldTicket, newTicket, userId);

                // ticket notification
                BTUser btUser = await _userManager.GetUserAsync(User);

                BTUser? projectManager = await _btProjectService.GetProjectManagerAsync(ticket.ProjectId);

                Notification notification = new()
                {
                    NotificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(n => n.Name == nameof(BTNotificationTypes.Ticket)))!.Id,
                    TicketId = ticket.Id,
                    Title = "Ticket Updated",
                    Message = $"Ticket : {ticket.Title}, was edited by {btUser.FullName}",
                    Created = SetDate.Format(DateTime.Now),
                    SenderId = userId,
                    RecipientId = projectManager?.Id
                };

                // test if PM is null
                if (projectManager != null)
                {
                    await _btNotificationService.AddNotificationAsync(notification);
                    await _btNotificationService.SendEmailNotificationAsync(notification, $"Ticket Edited for Project: {ticket.Project!.Name}");
                }
                else
                {
                    // send email to admins
                    await _btNotificationService.AdminNotificationAsync(notification, companyId);
                    await _btNotificationService.SendAdminEmailNotificationAsync(notification, $"Ticket Edited for Project: {ticket.Project!.Name}", companyId);
                }
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

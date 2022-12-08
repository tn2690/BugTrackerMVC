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
using BugTrackerMVC.Services.Interfaces;

namespace BugTrackerMVC.Controllers
{
    public class TicketCommentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _btTicketService;

        public TicketCommentsController(ApplicationDbContext context,
                                        UserManager<BTUser> userManager,
                                        IBTTicketService btTicketService)
        {
            _context = context;
            _userManager = userManager;
            _btTicketService = btTicketService;
        }

        // GET: TicketComments
        public async Task<IActionResult> Index()
        {
            List<TicketComment> ticketComments = await _context.TicketComments.Include(t => t.BTUser).Include(t => t.Ticket).ToListAsync();

            return View(ticketComments);
        }

        // GET: TicketComments/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.TicketComments == null)
            {
                return NotFound();
            }

            TicketComment? ticketComment = await _context.TicketComments
                .Include(t => t.BTUser)
                .Include(t => t.Ticket)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticketComment == null)
            {
                return NotFound();
            }

            return View(ticketComment);
        }

        // GET: TicketComments/Create
        public IActionResult Create()
        {
            ViewData["BTUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description");

            return View();
        }

        // POST: TicketComments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Comment,TicketId")] TicketComment ticketComment)
        {
            // remove BT user id
            ModelState.Remove("BTUserId");

            if (ModelState.IsValid)
            {
                // assign author of comment to BTUserId
                ticketComment.BTUserId = _userManager.GetUserId(User);

                // set date creation of comment
                ticketComment.Created = DateTime.UtcNow;

                await _btTicketService.AddCommentAsync(ticketComment);

                return RedirectToAction("Details", "Tickets", new { id = ticketComment.TicketId });
            }

            ViewData["BTUserId"] = new SelectList(_context.Users, "Id", "Id", ticketComment.BTUserId);
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketComment.TicketId);
            
            return View(ticketComment);
        }

        // GET: TicketComments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.TicketComments == null)
            {
                return NotFound();
            }

            TicketComment? ticketComment = await _context.TicketComments.FindAsync(id);

            if (ticketComment == null)
            {
                return NotFound();
            }

            ViewData["BTUserId"] = new SelectList(_context.Users, "Id", "Id", ticketComment.BTUserId);
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketComment.TicketId);

            return View(ticketComment);
        }

        // POST: TicketComments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Comment,Created,TicketId,BTUserId")] TicketComment ticketComment)
        {
            if (id != ticketComment.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(ticketComment);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketCommentExists(ticketComment.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["BTUserId"] = new SelectList(_context.Users, "Id", "Id", ticketComment.BTUserId);
            ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketComment.TicketId);

            return View(ticketComment);
        }

        // GET: TicketComments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.TicketComments == null)
            {
                return NotFound();
            }

            TicketComment? ticketComment = await _context.TicketComments
                .Include(t => t.BTUser)
                .Include(t => t.Ticket)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (ticketComment == null)
            {
                return NotFound();
            }

            return View(ticketComment);
        }

        // POST: TicketComments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.TicketComments == null)
            {
                return Problem("Entity set 'ApplicationDbContext.TicketComments'  is null.");
            }

            TicketComment? ticketComment = await _context.TicketComments.FindAsync(id);

            if (ticketComment != null)
            {
                _context.TicketComments.Remove(ticketComment);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketCommentExists(int id)
        {
          return _context.TicketComments.Any(e => e.Id == id);
        }
    }
}

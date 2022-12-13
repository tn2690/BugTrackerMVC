using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTrackerMVC.Data;
using BugTrackerMVC.Models;
using Microsoft.AspNetCore.Authorization;
using BugTrackerMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using BugTrackerMVC.Extensions;
using BugTrackerMVC.Services;
using System.ComponentModel.Design;

namespace BugTrackerMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class InvitesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTProjectService _btProjectService;
        private readonly IBTCompanyService _btCompanyService;
        private readonly IEmailSender _btEmailService;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTInviteService _btInviteService;
        private readonly IDataProtector _protector;
        

        public InvitesController(ApplicationDbContext context,
                                 IBTProjectService btProjectService,
                                 IBTCompanyService btCompanyService,
                                 IEmailSender emailSender,
                                 UserManager<BTUser> userManager,
                                 IBTInviteService btInviteService,
                                 IDataProtectionProvider dataProtectionProvider)
        {
            _context = context;
            _btProjectService = btProjectService;
            _btCompanyService = btCompanyService;
            _btEmailService = emailSender;
            _userManager = userManager;
            _btInviteService = btInviteService;
            _protector = dataProtectionProvider.CreateProtector("TN.BugTr@cker.2022");

        }

        // GET: Invites
        public async Task<IActionResult> Index()
        {
            List<Invite> invites = await _context.Invites
                                                .Include(i => i.Company)
                                                .Include(i => i.Invitee)
                                                .Include(i => i.Invitor)
                                                .Include(i => i.Project)
                                                .ToListAsync();

            return View(invites);
        }

        // GET: Invites/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Invites == null)
            {
                return NotFound();
            }

            Invite? invite = await _context.Invites
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .Include(i => i.Project)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ProcessInvite(string token, string email, string company)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(company))
            {
                return NotFound();
            }

            Guid companyToken = Guid.Parse(_protector.Unprotect(token));
            string? inviteeEmail = _protector.Unprotect(email);
            int companyId = int.Parse(_protector.Unprotect(company));

            try
            {
                Invite? invite = await _btInviteService.GetInviteAsync(companyToken, inviteeEmail, companyId);

                if (invite != null)
                {
                    return View(invite);
                }

                return NotFound();

            }
            catch (Exception)
            {
                throw;
            }

        }

        // GET: Invites/Create
        public IActionResult Create()
        {
            int companyId = User.Identity!.GetCompanyId();

            //ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name");
            //ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id");
            //ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description");

            return View();
        }

        // POST: Invites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> Create([Bind("Id,InviteeEmail,InviteeFirstName,InviteeLastName,Message")] Invite invite)
        //{
        //    ModelState.Remove("InvitorId");

        //    int companyId = User.Identity!.GetCompanyId();

        //    if (ModelState.IsValid)
        //    {
        //        try
        //        {


        //            // encrypt code for invite
        //            Guid guid = Guid.NewGuid();

        //            string token = _protector.Protect(guid.ToString());
        //            string email = _protector.Protect(invite.InviteeEmail!);
        //            string company = _protector.Protect(companyId.ToString());

        //            string? callbackUrl = Url.Action("ProcessInvite", "Invites", new { token, email, company }, protocol: Request.Scheme);

        //            string body = $@"{invite.Message} <br />
        //               Please join my Company. <br />
        //               Click the following link to join our team. <br />
        //               <a href=""{callbackUrl}"">COLLABORATE</a>";
        //            string? destination = invite.InviteeEmail;

        //            Company btCompany = await _btCompanyService.GetCompanyInfoAsync(companyId);

        //            string? subject = $" Nova Tracker: {btCompany.Name} Invite";

        //            await _btEmailService.SendEmailAsync(destination, subject, body);

        //            // save invite in db
        //            invite.CompanyToken = guid;
        //            invite.CompanyId = companyId;
        //            invite.InviteDate = DataUtility.GetPostGresDate(DateTime.Now);
        //            invite.InvitorId = _userManager.GetUserId(User);
        //            invite.IsValid = true;

        //            // Add Invite service method for "AddNewInviteAsync"
        //            await _btInviteService.AddNewInviteAsync(invite);

        //            return RedirectToAction("Index", "Home");

        //            // TODO: Possibly use SWAL message
        //        }
        //        catch (Exception)
        //        {
        //            throw;
        //        }

        //        //_context.Add(invite);
        //        //await _context.SaveChangesAsync();
        //        //return RedirectToAction(nameof(Index));
        //    }

        //    //ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", invite.CompanyId);
        //    //ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
        //    //ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
        //    ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", invite.ProjectId);

        //    return View(invite);
        //}

        // GET: Invites/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Invites == null)
            {
                return NotFound();
            }

            Invite? invite = await _context.Invites.FindAsync(id);

            if (invite == null)
            {
                return NotFound();
            }

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", invite.CompanyId);
            ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
            ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", invite.ProjectId);

            return View(invite);
        }

        // POST: Invites/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,InviteDate,JoinDate,CompanyToken,InviteeEmail,InviteeFirstName,InviteeLastName,Message,IsValid,CompanyId,ProjectId,InvitorId,InviteeId")] Invite invite)
        {
            if (id != invite.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invite);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InviteExists(invite.Id))
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

            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", invite.CompanyId);
            ViewData["InviteeId"] = new SelectList(_context.Users, "Id", "Id", invite.InviteeId);
            ViewData["InvitorId"] = new SelectList(_context.Users, "Id", "Id", invite.InvitorId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", invite.ProjectId);

            return View(invite);
        }

        // GET: Invites/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Invites == null)
            {
                return NotFound();
            }

            Invite? invite = await _context.Invites
                .Include(i => i.Company)
                .Include(i => i.Invitee)
                .Include(i => i.Invitor)
                .Include(i => i.Project)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (invite == null)
            {
                return NotFound();
            }

            return View(invite);
        }

        // POST: Invites/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Invites == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Invites'  is null.");
            }

            Invite? invite = await _context.Invites.FindAsync(id);

            if (invite != null)
            {
                _context.Invites.Remove(invite);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InviteExists(int id)
        {
          return _context.Invites.Any(e => e.Id == id);
        }
    }
}

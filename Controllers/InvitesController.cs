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
using BugTrackerMVC.Helpers;
using BugTrackerMVC.Services;
using System.ComponentModel.Design;
using Microsoft.AspNetCore.DataProtection.Infrastructure;

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
            _protector = dataProtectionProvider.CreateProtector("TN.BugTr@ckerMvC.2022");

        }

        // GET: Invites
        public async Task<IActionResult> Index(string? swalInviteMsg = null)
        {
            // sweet alert message
            ViewData["SwalMessage"] = swalInviteMsg;

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

        // GET: Invites/Create
        public async Task<IActionResult> Create()
        {
            int companyId = User.Identity!.GetCompanyId();

            ViewData["ProjectId"] = new SelectList(await _btProjectService.GetAllProjectsByCompanyIdAsync(companyId), "Id", "Name");

            return View(new Invite());
        }

        // POST: Invites/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,ProjectId,InviteeEmail,InviteeFirstName,InviteeLastName,Message")] Invite invite)
        {
            ModelState.Remove("InvitorId");

            int companyId = User.Identity!.GetCompanyId();

            if (ModelState.IsValid)
            {
                try
                {
                    // encrypt code for invite
                    Guid guid = Guid.NewGuid();

                    string token = _protector.Protect(guid.ToString());
                    string email = _protector.Protect(invite.InviteeEmail!);
                    string company = _protector.Protect(companyId.ToString());

                    string? callbackUrl = Url.Action("ProcessInvite", "Invites", new { token, email, company }, protocol: Request.Scheme);

                    string body = $@"{invite.Message} <br />
                       You've received an invitation to join our company! <br />
                       Click the following link to join our team. <br />
                       <a href=""{callbackUrl}"">COLLABORATE</a>";

                    string? destination = invite.InviteeEmail;

                    Company btCompany = await _btCompanyService.GetCompanyInfoAsync(companyId);

                    string? subject = $" Insect Crusher: {btCompany.Name} Exclusive Invite!";

                    // sweet alert
                    string? swalInviteMsg = string.Empty;

                    await _btEmailService.SendEmailAsync(destination, subject, body);

                    // save invite in the DB
                    invite.CompanyToken = guid;
                    invite.CompanyId = companyId;
                    invite.InviteDate = SetDate.Format(DateTime.Now);
                    invite.InvitorId = _userManager.GetUserId(User);
                    invite.IsValid = true;

                    // add Invite service method for "AddNewInviteAsync"
                    await _btInviteService.AddNewInviteAsync(invite);

                    swalInviteMsg = "Success: Invite Sent!";

                    return RedirectToAction("Index", "Invites", new { swalInviteMsg });

                } catch (Exception)
                {
                    throw;
                }

            }

            ViewData["ProjectId"] = new SelectList(await _btProjectService.GetAllProjectsByCompanyIdAsync(companyId), "Id", "Name");

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

            } catch (Exception)
            {
                throw;
            }
        }
    }
}

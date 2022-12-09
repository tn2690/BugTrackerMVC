using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTrackerMVC.Data;
using BugTrackerMVC.Models;
using BugTrackerMVC.Models.ViewModels;
using BugTrackerMVC.Extensions;
using BugTrackerMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;

namespace BugTrackerMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTCompanyService _btCompanyService;
        private readonly IBTRolesService _btRolesService;
        private readonly UserManager<BTUser> _userManager;

        public CompaniesController(ApplicationDbContext context,
                                   IBTCompanyService btCompanyService,
                                   IBTRolesService btRolesService,
                                   UserManager<BTUser> userManager)
        {
            _context = context;
            _btCompanyService = btCompanyService;
            _btRolesService = btRolesService;
            _userManager = userManager;
        }

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            Company? company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);

            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            // add instance of VM as a list (model)
            List<ManageUserRolesViewModel> model = new();

            // get company id
            int companyId = User.Identity!.GetCompanyId();

            // get all company users
            List<BTUser> members = await _btCompanyService.GetMembersAsync(companyId);

            string btUserId = _userManager.GetUserId(User);

            // loop over users to populate the VM
            foreach (BTUser member in members)
            {
                if (string.Compare(btUserId, member.Id) != 0)
                {
                    // instantiate single VM
                    ManageUserRolesViewModel viewModel = new();

                    // get current roles
                    IEnumerable<string> currentRoles = await _btRolesService.GetUserRolesAsync(member);

                    // viewModel to model
                    viewModel.BTUser = member;

                    // create multi-select
                    viewModel.Roles = new MultiSelectList(await _btRolesService.GetRolesAsync(), "Name", "Name", currentRoles);

                    model.Add(viewModel);
                }
            }

            // return model to the view
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel viewModel)
        {
            // get company id
            int companyId = User.Identity!.GetCompanyId();

            // instantiate btUser
            BTUser? btUser = (await _btCompanyService.GetMembersAsync(companyId)).FirstOrDefault(m => m.Id == viewModel.BTUser!.Id);

            // get roles for user
            IEnumerable<string> currentRoles = await _btRolesService.GetUserRolesAsync(btUser!);

            // get selected roles for user
            string? selectedRole = viewModel.SelectedRoles!.FirstOrDefault();

            // remove current role(s) and add new role
            if (!string.IsNullOrEmpty(selectedRole))
            {
                if (await _btRolesService.RemoveUserFromRolesAsync(btUser!, currentRoles))
                {
                    await _btRolesService.AddUserToRoleAsync(btUser!, selectedRole);
                }
            }

            // navigate
            return RedirectToAction(nameof(ManageUserRoles));
        }
    }
}

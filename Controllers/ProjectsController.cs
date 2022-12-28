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
using BugTrackerMVC.Helpers;
using Microsoft.AspNetCore.Authorization;
using BugTrackerMVC.Extensions;
using BugTrackerMVC.Models.ViewModels;
using BugTrackerMVC.Models.Enums;
using BugTrackerMVC.Services;

namespace BugTrackerMVC.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _btFileService;
        private readonly IBTProjectService _btProjectService;
        private readonly IBTRolesService _btRolesService;

        public ProjectsController(ApplicationDbContext context,
                                  UserManager<BTUser> userManager,
                                  IBTFileService btFileService,
                                  IBTProjectService btProjectService,
                                  IBTRolesService btRolesService)
        {
            _context = context;
            _userManager = userManager;
            _btFileService = btFileService;
            _btProjectService = btProjectService;
            _btRolesService = btRolesService;
        }

        // GET: Projects/AllProjects
        public async Task<IActionResult> AllProjects()
        {
            // assign user's company id to logged in user
            int companyId = User.Identity!.GetCompanyId();

            // show projects for specific company
            List<Project> projects = (await _btProjectService.GetAllProjectsByCompanyIdAsync(companyId)).ToList();

            return View(projects);
        }

        //  GET: Projects/ArchivedProjects
        //  for Admin and Project Managers to view
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> ArchivedProjects()
        {
            // assign user's company id to logged in user
            int companyId = User.Identity!.GetCompanyId();

            // show archived projects for specific company
            List<Project> projects = (await _btProjectService.GetArchivedProjectByCompanyIdAsync(companyId)).ToList();

            return View(projects);
        }

        // GET: Projects/MyProjects
        public async Task<IActionResult> MyProjects()
        {
            // assign user's company id to logged in user
            int companyId = User.Identity!.GetCompanyId();

            List<Project>? projects = new();

            if (User.IsInRole("Admin"))
            {
                // show projects for specific company
                projects = await _btProjectService.GetAllProjectsByCompanyIdAsync(companyId);
            } 
            else
            {
                string userId = _userManager.GetUserId(User);

                // make call to service
                projects = await _btProjectService.GetUserProjectsAsync(userId);
            }

            return View(projects);
        }

        // GET: Projects/AddMembers/5
        [HttpGet]
        public async Task<IActionResult> AddMembers(int? id)
        {
            // validate id
            if (id == null)
            {
                return NotFound();
            }

            // get company id
            int companyId = User.Identity!.GetCompanyId();

            Project? project = await _btProjectService.GetProjectByIdAsync(id.Value, companyId);

            // get developers and submitters from project
            List<BTUser> projectDevelopers = new List<BTUser>();
            List<BTUser> projectSubmitters = new List<BTUser>();

            foreach (BTUser member in project.Members)
            {
                if (await _btRolesService.IsUserInRoleAsync(member, nameof(BTRoles.Submitter)))
                {
                    projectSubmitters.Add(member);
                }
                else if (await _btRolesService.IsUserInRoleAsync(member, nameof(BTRoles.Developer)))
                {
                    projectDevelopers.Add(member);
                }
                else
                {
                    continue;
                }
            }

            // list of developers
            List<BTUser> developers = await _btRolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);

            // list of submitters
            List<BTUser> submitters = await _btRolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId);

            // get and assign Project property of view model
            // need member and project id
            AssignPMViewModel viewModel = new()
            {
                Project = await _btProjectService.GetProjectByIdAsync(id.Value, companyId),
                DevList = new MultiSelectList(developers, "Id", "FullName", projectDevelopers.Select(u => u.Id)), // create MultiSelectList of company's Devs
                SubList = new MultiSelectList(submitters, "Id", "FullName", projectSubmitters.Select(u => u.Id)),
                SelectedDevelopers = projectDevelopers.Select(u => u.Id).ToList(),
                SelectedSubmitters = projectSubmitters.Select(u => u.Id).ToList()
            };

            return View(viewModel);
        }

        // POST: Projects/AddMembers/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMembers(AssignPMViewModel model)
        { 
            // check if project exists
            if (model.Project?.Id != null)
            {
                Project? project = await _btProjectService.GetProjectByIdAsync(model.Project.Id, User.Identity!.GetCompanyId());

                // remove all current members
                foreach (BTUser member in project.Members)
                {
                    if (await _btRolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager))) continue;
                    else await _btProjectService.RemoveMemberFromProjectAsync(member, project.Id);
                }

                // combine developers and submitters
                List<string> selectedMembers = model.SelectedDevelopers;
                selectedMembers.AddRange(model.SelectedSubmitters);

                // add them back to the project
                foreach (string memberId in selectedMembers)
                {
                    BTUser member = await _userManager.FindByIdAsync(memberId);
                    await _btProjectService.AddMemberToProjectAsync(member, project.Id);
                }

                return RedirectToAction(nameof(Details), new { id = model.Project?.Id });
            }

            return View(model);
        }

        // GET: Projects/AssignProjectManager/5
        [HttpGet]
        [Authorize(Roles = nameof(BTRoles.Admin))]
        public async Task<IActionResult> AssignProjectManager(int? id)
        {
            // validate id
            if (id == null)
            {
                return NotFound();
            }

            // get company id
            int companyId = User.Identity!.GetCompanyId();

            List<BTUser> projectManagers = await _btRolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId);

            BTUser? currentPM = await _btProjectService.GetProjectManagerAsync(id.Value);

            // create/instantiate PMViewModel
            // get and assign Project property of view model
            AssignPMViewModel viewModel = new()
            {
                Project = await _btProjectService.GetProjectByIdAsync(id.Value, companyId),
                PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id), // create SelectList of company's PMs (highlight current PM if one is assigned)
                PMId = currentPM?.Id
            };

            // return View() using AssignPMViewModel
            return View(viewModel);
        }

        // POST: Projects/AssignProjectManager/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignProjectManager(AssignPMViewModel viewModel)
        {
            if (viewModel.Project?.Id != null)
            {
                // validate PM id of viewModel
                if (!string.IsNullOrEmpty(viewModel.PMId))
                {
                    // call service to add PM
                    await _btProjectService.AddProjectManagerAsync(viewModel.PMId, viewModel.Project.Id);
                }
                else
                {
                    // call service to remove PM
                    await _btProjectService.RemoveProjectManagerAsync(viewModel.Project.Id);
                }

                return RedirectToAction(nameof(Details), new { id = viewModel.Project?.Id });
            }

            // return View() using AssignPMViewModel
            return View(viewModel);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // assign user's company id to logged in user
            int companyId = User.Identity!.GetCompanyId();

            Project? project = await _btProjectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Create()
        {
            ViewData["ProjectPriorityId"] = new SelectList(await _btProjectService.GetProjectPrioritiesAsync(), "Id", "Name");

            return View(new Project());
        }

        // POST: Projects/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageFormFile")] Project project)
        {
            ModelState.Remove("CompanyId");

            if (ModelState.IsValid)
            {
                // assign user's company id to logged in user
                int companyId = User.Identity!.GetCompanyId();

                project.CompanyId = companyId;

                // set dates for Created, StartDate, EndDate
                project.Created = DateTime.UtcNow;

                project.StartDate = SetDate.Format(project.StartDate);

                project.EndDate = SetDate.Format(project.EndDate);

                // set image
                // check whether an image has been uploaded
                if (project.ImageFormFile != null)
                {
                    // convert file to byte array
                    project.ImageFileData = await _btFileService.ConvertFileToByteArrayAsync(project.ImageFormFile);

                    // use file extension as the file
                    project.ImageFileType = project.ImageFormFile.ContentType;
                }

                await _btProjectService.AddProjectAsync(project);
                
                return RedirectToAction(nameof(AllProjects));
            }

            ViewData["ProjectPriorityId"] = new SelectList(await _btProjectService.GetProjectPrioritiesAsync(), "Id", "Name");

            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            // call service
            //Project? project = await _context.Projects.FindAsync(id);
            Project? project = await _btProjectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            ViewData["ProjectPriorityId"] = new SelectList(await _btProjectService.GetProjectPrioritiesAsync(), "Id", "Name");

            return View(project);
        }

        // POST: Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,StartDate,EndDate,ProjectPriorityId,ImageFormFile,ImageFileData,ImageFileType")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // set dates for Created, StartDate, EndDate
                    project.Created = SetDate.Format(DateTime.UtcNow);

                    project.StartDate = SetDate.Format(project.StartDate);

                    project.EndDate = SetDate.Format(project.EndDate);

                    // set image
                    // check whether an image has been uploaded
                    if (project.ImageFormFile != null)
                    {
                        // convert file to byte array
                        project.ImageFileData = await _btFileService.ConvertFileToByteArrayAsync(project.ImageFormFile);

                        // use file extension as the file
                        project.ImageFileType = project.ImageFormFile.ContentType;
                    }

                    // call service
                    await _btProjectService.UpdateProjectAsync(project);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(AllProjects));
            }

            //ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);

            ViewData["ProjectPriorityId"] = new SelectList(await _btProjectService.GetProjectPrioritiesAsync(), "Id", "Name");

            return View(project);
        }

        // GET: Projects/Archive/5
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // assign user's company id to logged in user
            int companyId = User.Identity!.GetCompanyId();

            Project? project = await _btProjectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Restore/5
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // assign user's company id to logged in user
            int companyId = User.Identity!.GetCompanyId();

            Project? project = await _btProjectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            //BTUser member = await _userManager.GetUserAsync(User);

            Project? project = await _btProjectService.GetProjectByIdAsync(id, companyId);

            // remove a member from a project
            //await _btProjectService.RemoveMemberFromProjectAsync(member, project.Id);

            if (project != null)
            {
                // call service (UpdateProjectAsync)
                // set Archived property to true
                project.Archived = true;
                // send project to service for update
                await _btProjectService.UpdateProjectAsync(project);
            }
            
            return RedirectToAction(nameof(AllProjects));
        }

        // POST: Projects/Restore/5
        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles="Admin")]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            Project? project = await _btProjectService.GetProjectByIdAsync(id, companyId);

            if (project != null)
            {
                // call service (UpdateProjectAsync)
                // set Archived property to false
                project.Archived = false;
                // send project to service for update
                await _btProjectService.UpdateProjectAsync(project);
            }

            return RedirectToAction(nameof(ArchivedProjects));
        }

        private async Task<bool> ProjectExists(int id)
        {
            int companyId = User.Identity!.GetCompanyId();

            return (await _btProjectService.GetAllProjectsByCompanyIdAsync(companyId)).Any(e => e.Id == id);
        }
    }
}

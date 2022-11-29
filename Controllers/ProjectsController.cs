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
using BugTrackerMVC.Helper;
using Microsoft.AspNetCore.Authorization;

namespace BugTrackerMVC.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IFileService _fileService;
        private readonly IBTProjectService _btProjectService;

        public ProjectsController(ApplicationDbContext context,
                                  UserManager<BTUser> userManager,
                                  IFileService fileService,
                                  IBTProjectService btProjectService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
            _btProjectService = btProjectService;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            // assign user's company id to logged in user
            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            // show projects for specific company
            List<Project> projects = (await _btProjectService.GetAllProjectsByCompanyIdAsync(companyId)).ToList();

            return View(projects);
        }

        //  GET: Projects/ArchivedProjects
        //  for Admin and Project Managers to view
        public async Task<IActionResult> ArchivedProjects()
        {
            // assign user's company id to logged in user
            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            // show archived projects for specific company
            List<Project> projects = (await _btProjectService.GetArchivedProjectsByCompanyIdAsync(companyId)).ToList();

            return View(projects);
        }

        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Project? project = await _btProjectService.GetProjectDetailsByIdAsync(id);

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
            //// get logged in user
            //BTUser btUserId = await _userManager.GetUserAsync(User);

            //// assign user's company id
            //int companyId = btUserId.CompanyId;

            // match to project
            //List<Company> company = (await _bugTrackerService.GetCompanyAsync(companyId)).ToList();

            //ViewData["CompanyId"] = new SelectList(company, "Id", "Name");
            // TODO: call Project Service
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name");

            return View(new Project());
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,ProjectManager")]
        public async Task<IActionResult> Create([Bind("Id,CompanyId,Name,Description,StartDate,EndDate,ProjectPriorityId,Archived,ImageFormFile")] Project project)
        {
            if (ModelState.IsValid)
            {
                // assign user's company id to logged in user
                int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

                project.CompanyId = companyId;

                // set dates for Created, StartDate, EndDate
                project.Created = PostgresDate.Format(DateTime.Now);

                project.StartDate = PostgresDate.Format(DateTime.Now);

                if (project.EndDate != null) 
                { 
                    project.EndDate = DateTime.SpecifyKind(project.EndDate.Value, DateTimeKind.Utc);
                }

                // set image
                // check whether an image has been uploaded
                if (project.ImageFormFile != null)
                {
                    // convert file to byte array
                    project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);

                    // use file extension as the file
                    project.ImageFileType = project.ImageFormFile.ContentType;
                }

                await _btProjectService.AddProjectAsync(project);
                
                return RedirectToAction(nameof(Index));
            }

            //// get logged in user
            //BTUser btUserId = await _userManager.GetUserAsync(User);

            // assign user's company id
            //int companyId = btUserId.CompanyId;

            // match to project
            //List<Company> company = (await _bugTrackerService.GetCompanyAsync(companyId)).ToList();

            //ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);

            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // TODO: call service
            //Project? project = await _context.Projects.FindAsync(id);
            Project? project = await _btProjectService.GetProjectByIdAsync(id);

            if (project == null)
            {
                return NotFound();
            }

            //// get logged in user
            //BTUser btUserId = await _userManager.GetUserAsync(User);

            //// assign user's company id
            //int companyId = btUserId.CompanyId;

            //// match to project
            //List<Company> company = (await _bugTrackerService.GetCompanyAsync(companyId)).ToList();

            //ViewData["CompanyId"] = new SelectList(company, "Id", "Name", companyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);

            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageFormFile,ImageFileData,ImageFileType,Archived")] Project project)
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
                    project.Created = PostgresDate.Format(DateTime.Now);

                    project.StartDate = PostgresDate.Format(DateTime.Now);

                    if (project.EndDate != null)
                    {
                        project.EndDate = DateTime.SpecifyKind(project.EndDate.Value, DateTimeKind.Utc);
                    }

                    // set image
                    // check whether an image has been uploaded
                    if (project.ImageFormFile != null)
                    {
                        // convert file to byte array
                        project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);

                        // use file extension as the file
                        project.ImageFileType = project.ImageFormFile.ContentType;
                    }

                    await _btProjectService.UpdateProjectAsync(project);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
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

            //ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);

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

            Project? project = await _btProjectService.GetProjectDetailsByIdAsync(id);

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

            Project? project = await _btProjectService.GetProjectDetailsByIdAsync(id);

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
        public async Task<IActionResult> ArchivedConfirmed(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            Project? project = await _btProjectService.GetProjectByIdAsync(id);

            if (project != null)
            {
                // TODO: call service (ArchiveProjectAsync)
                // set Archived property to true
                // send project to service for update
                await _btProjectService.ArchiveProjectAsync(project);
            }
            
            return RedirectToAction(nameof(Index));
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

            Project? project = await _btProjectService.GetProjectByIdAsync(id);

            if (project != null)
            {
                // TODO: call service (RestoreProjectAsync)
                // set Archived property to false
                // send project to service for update
                await _btProjectService.RestoreProjectAsync(project);
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
          return _context.Projects.Any(e => e.Id == id);
        }
    }
}

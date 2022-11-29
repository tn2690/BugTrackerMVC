using BugTrackerMVC.Data;
using BugTrackerMVC.Models;
using BugTrackerMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BugTrackerMVC.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;
        private readonly UserManager<BTUser> _userManager;
        
        public BTProjectService(ApplicationDbContext context,
                                IBTRolesService rolesService,
                                UserManager<BTUser> userManager)
        {
            _context = context;
            _rolesService = rolesService;
            _userManager = userManager;
        }

        public async Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Project> projects = await _context.Projects
                                                       .Where(p => p.CompanyId == companyId && p.Archived == false)
                                                       .Include(p => p.Company)
                                                       .Include(p => p.ProjectPriority)
                                                       .Include(p => p.Tickets)
                                                       .Include(p => p.Members)
                                                       .OrderByDescending(p => p.ProjectPriority)
                                                            .ThenByDescending(p => p.Created)
                                                       .ToListAsync();

                return projects;

            } catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Project> projects = await _context.Projects
                                                       .Where(p => p.Archived == true && p.CompanyId == companyId)
                                                       .Include(p => p.Company)
                                                       .Include(p => p.ProjectPriority)
                                                       .OrderByDescending(p => p.ProjectPriority)
                                                            .ThenByDescending(p => p.Created)
                                                       .ToListAsync();

                return projects;

            }
            catch (Exception)
            {
                throw;
            }
        }

        //public async Task<Project> GetProjectDetailsByIdAsync(int? id)
        //{
        //    try
        //    {
        //        Project? project = await _context.Projects
        //                                        .Include(p => p.Company)
        //                                        .Include(p => p.ProjectPriority)
        //                                        .FirstOrDefaultAsync(m => m.Id == id);

        //        return project;

        //    } catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public async Task AddProjectAsync(Project project)
        {
            try
            {
                _context.Add(project);
                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Project> GetProjectByIdAsync(int? projectId, int companyId)
        {
            try
            {
                //Project? project = await _context.Projects
                //                            .FindAsync(projectId);

                Project? project = await _context.Projects
                                                 .Include(p => p.Company)
                                                 .Include(p => p.Members)
                                                 .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.Comments)
                                                 .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.Attachments)
                                                 .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.History)
                                                 .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketPriority)
                                                 .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketStatus)
                                                 .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketType)
                                                 .Include(p => p.ProjectPriority)
                                                 .FirstOrDefaultAsync(p => p.Id == projectId && p.CompanyId == companyId);

                return project;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateProjectAsync(Project project)
        {
            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {
                throw;
            }
        }

        //public async Task ArchiveProjectAsync(Project project)
        //{
        //    try
        //    {
        //        project.Archived = true;

        //        _context.Update(project);

        //        await _context.SaveChangesAsync();

        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public async Task RestoreProjectAsync(Project project)
        //{
        //    try
        //    {
        //        project.Archived = false;

        //        _context.Update(project);

        //        await _context.SaveChangesAsync();

        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public async Task<IEnumerable<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            try
            {
                List<ProjectPriority> projectPriorities = await _context.ProjectPriorities
                                                                        .ToListAsync();

                return projectPriorities;

            } catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            try
            {

                return true;

            } catch (Exception)
            {
                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            try
            {


            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> AddMemberToProjectAsync(BTUser member, int projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, member.CompanyId);

                bool IsOnProject = project.Members.Any(m => m.Id == member.Id);

                if (!IsOnProject)
                {
                    project.Members.Add(member);
                    
                    await _context.SaveChangesAsync();

                    return true;
                }

                return false;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RemoveMemberFromProjectAsync(BTUser member, int projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, member.CompanyId);

                bool IsOnProject = project.Members.Any(m => m.Id == member.Id);

                if (!IsOnProject)
                {
                    project.Members.Any(m => m.Id == member.Id);

                    await _context.SaveChangesAsync();

                    return false;
                }

                return false;

            }
            catch (Exception)
            {
                throw;
            }
        }





        //public async Task<List<Project>> GetProjectsUserAsync(int userId)
        //{
        //    try
        //    {
        //        List<Project> projects = await _context.Projects
        //                                        .Where(p => p.Members == userId)
        //                                        .ToListAsync();

        //        return projects;

        //    } catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        //public async Task<List<Company>> GetCompanyAsync(int companyId)
        //{
        //    List<Company> company = new List<Company>();

        //    try
        //    {

        //        company = await _context.Companies.Where(c => c.Id == companyId)
        //                                            .ToListAsync();

        //        return company;

        //    } catch (Exception)
        //    {
        //        throw;
        //    }
        //}

    }
}

using BugTrackerMVC.Data;
using BugTrackerMVC.Models;
using BugTrackerMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BugTrackerMVC.Enums;

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
                                                       .Where(p => p.CompanyId == companyId)
                                                       .Include(p => p.Company)
                                                       .Include(p => p.ProjectPriority)
                                                       .Include(p => p.Tickets)
                                                       .Include(p => p.Members)
                                                       .OrderByDescending(p => p.ProjectPriority)
                                                            .ThenByDescending(p => p.Archived)
                                                            .ThenByDescending(p => p.Created)
                                                       .ToListAsync();

                return projects;

            } catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Project>?> GetUserProjectsAsync(string userId)
        {
            try
            {
                List<Project>? projects = (await _context.Users
                                                        .Include(u => u.Projects)
                                                            .ThenInclude(p => p.ProjectPriority)
                                                        .Include(u => u.Projects)
                                                            .ThenInclude(p => p.Members)
                                                        .Include(u => u.Projects)
                                                            .ThenInclude(p => p.Tickets)
                                                        .FirstOrDefaultAsync(u => u.Id == userId))?
                                                        .Projects
                                                        .ToList();

                return projects;

            } catch (Exception)
            {
                throw;
            }
        }

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

        public async Task<Project> GetProjectByIdAsync(int projectId, int companyId)
        {
            try
            {
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

                return project!;

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

        public async Task<IEnumerable<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            try
            {
                IEnumerable<ProjectPriority> projectPriorities = await _context.ProjectPriorities
                                                                        .ToListAsync();

                return projectPriorities;

            } catch (Exception)
            {
                throw;
            }
        }

        public async Task<BTUser> GetProjectManagerAsync(int projectId)
        {
            try
            {
                Project? project = await _context.Projects
                                                .Include(p => p.Members)
                                                .FirstOrDefaultAsync(p => p.Id == projectId);

                foreach(BTUser member in project!.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        return member;
                    }
                }

                return null!;

            } catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> AddProjectManagerAsync(string userId, int projectId)
        {
            try
            {
                BTUser? currentPM = await GetProjectManagerAsync(projectId);
                BTUser? selectedPM = await _context.Users.FindAsync(userId);

                // remove current PM
                if (currentPM != null)
                {
                    await RemoveProjectManagerAsync(projectId);
                }

                // add new PM
                // try to add a member to project
                try
                {
                    await AddMemberToProjectAsync(selectedPM!, projectId);

                    await _context.SaveChangesAsync();

                    return true;

                } catch (Exception)
                {
                    throw;
                }

            } catch (Exception)
            {
                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int projectId)
        {
            try
            {
                BTUser? currentPM = await GetProjectManagerAsync(projectId);

                // remove current PM
                if (currentPM != null)
                {
                    await RemoveProjectManagerAsync(projectId);

                    await _context.SaveChangesAsync();
                }
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

                if (IsOnProject)
                {
                    project.Members.Remove(member);

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
    }
}

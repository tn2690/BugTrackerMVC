using BugTrackerMVC.Data;
using BugTrackerMVC.Models;
using BugTrackerMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTrackerMVC.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        
        public BTProjectService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Project> projects = await _context.Projects
                                                       .Where(p => p.CompanyId == companyId && p.Archived == false)
                                                       .Include(p => p.Company)
                                                       .Include(p => p.ProjectPriority)
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
                                                       .ToListAsync();

                return projects;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Project> GetProjectDetailsByIdAsync(int? id)
        {
            try
            {
                Project? project = await _context.Projects
                                                .Include(p => p.Company)
                                                .Include(p => p.ProjectPriority)
                                                .FirstOrDefaultAsync(m => m.Id == id);

                return project;

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

        public async Task<Project> GetProjectByIdAsync(int? projectId)
        {
            try
            {
                Project? project = await _context.Projects
                                            .FindAsync(projectId);

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

        public async Task ArchiveProjectAsync(Project project)
        {
            try
            {
                project.Archived = true;

                _context.Update(project);

                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task RestoreProjectAsync(Project project)
        {
            try
            {
                project.Archived = false;

                _context.Update(project);

                await _context.SaveChangesAsync();

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

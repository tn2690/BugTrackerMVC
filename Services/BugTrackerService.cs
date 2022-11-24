using BugTrackerMVC.Data;
using BugTrackerMVC.Models;
using BugTrackerMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTrackerMVC.Services
{
    public class BugTrackerService : IBugTrackerService
    {
        private readonly ApplicationDbContext _context;
        
        public BugTrackerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Project>> GetProjectsAsync(int companyId)
        {
            List<Project> projects = new List<Project>();

            try
            {
                projects = await _context.Projects
                                         .Where(p => p.CompanyId == companyId)
                                         .Include(p => p.Company)
                                         .Include(p => p.ProjectPriority)
                                         .ToListAsync();

                return projects;

            } catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Company>> GetCompanyAsync(int companyId)
        {
            List<Company> company = new List<Company>();

            try
            {

                company = await _context.Companies.Where(c => c.Id == companyId)
                                                    .ToListAsync();

                return company;

            } catch (Exception)
            {
                throw;
            }
        }



    }
}

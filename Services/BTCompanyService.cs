using BugTrackerMVC.Data;
using BugTrackerMVC.Models;
using BugTrackerMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugTrackerMVC.Services
{
    public class BTCompanyService : IBTCompanyService
    {
        private readonly ApplicationDbContext _context;

        public BTCompanyService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<BTUser>> GetMembersAsync(int? companyId)
        {
            try
            {
                List<BTUser> members = new();

                members = await _context.Users.Where(u => u.CompanyId == companyId)
                                              .ToListAsync();

                return members;

            } catch (Exception) 
            {
                throw;
            }
        }
    }
}

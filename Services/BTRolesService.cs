using BugTrackerMVC.Models;
using BugTrackerMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BugTrackerMVC.Services
{
    public class BTRolesService : IBTRolesService
    {
        private readonly UserManager<BTUser> _userManager;

        public BTRolesService(UserManager<BTUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId)
        {
            try
            {
                List<BTUser> result = new();
                List<BTUser> users = new();

                users = (await _userManager.GetUsersInRoleAsync(roleName)).ToList();

                result = users.Where(u => u.CompanyId == companyId).ToList();

                return result;

            } catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> IsUserInRoleAsync(BTUser member, string roleName)
        {
            try
            {
                bool result = await _userManager.IsInRoleAsync(member, roleName);

                return result;

            } catch (Exception)
            {
                throw;
            }
        }
    }
}

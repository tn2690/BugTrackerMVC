using BugTrackerMVC.Models;

namespace BugTrackerMVC.Services.Interfaces
{
    public interface IBTRolesService
    {
        // list of BT users
        public Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId);

        // check what role a user is in
        public Task<bool> IsUserInRoleAsync(BTUser member, string roleName);
    }
}

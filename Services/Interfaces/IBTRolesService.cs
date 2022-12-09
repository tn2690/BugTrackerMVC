using BugTrackerMVC.Models;
using Microsoft.AspNetCore.Identity;

namespace BugTrackerMVC.Services.Interfaces
{
    public interface IBTRolesService
    {
        // list of BT users
        public Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId);

        // check what role a user is in
        public Task<bool> IsUserInRoleAsync(BTUser member, string roleName);

        // add user to role
        public Task<bool> AddUserToRoleAsync(BTUser user, string roleName);

        /// <summary>
        /// Get All Roles
        /// </summary>
        /// <returns></returns>
        public Task<List<IdentityRole>> GetRolesAsync();

        /// <summary>
        /// Get the role(s) for the provided BTUser
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public Task<IEnumerable<string>> GetUserRolesAsync(BTUser user);

        /// <summary>
        /// Remove provided BTUser from a single role 
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName);

        /// <summary>
        /// Remove provided BTUser from the list of roles provided
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roleName"></param>
        /// <returns></returns>
        public Task<bool> RemoveUserFromRolesAsync(BTUser user, IEnumerable<string> roleName);
    }
}

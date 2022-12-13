using BugTrackerMVC.Models;

namespace BugTrackerMVC.Services.Interfaces
{
    public interface IBTCompanyService
    {
        public Task<List<BTUser>> GetMembersAsync(int? companyId);

        public Task<Company> GetCompanyInfoAsync(int? companyId);
    }
}

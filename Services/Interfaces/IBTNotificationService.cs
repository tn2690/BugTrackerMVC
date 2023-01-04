using BugTrackerMVC.Models;

namespace BugTrackerMVC.Services.Interfaces
{
    public interface IBTNotificationService
    {
        public Task AddNotificationAsync(Notification notification);

        public Task AdminNotificationAsync(Notification notification, int companyId);

        // get all notifications by user id
        public Task<List<Notification>> GetNotificationsByUserIdAsync(string userId);

        public Task<bool> SendAdminEmailNotificationAsync(Notification notification, string emailSubject, int companyId);

        public Task<bool> SendEmailNotificationAsync(Notification notification, string emailSubject);

        public Task<IEnumerable<NotificationType>> GetNotificationTypesAsync();

        public Task UpdateNotificationAsync(Notification notification);

        // get new notifications by user id
        public Task<List<Notification>> GetNewNotificationsByUserIdAsync(string userId);

    }
}

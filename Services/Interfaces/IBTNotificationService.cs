using BeastieHunter.Models;

namespace BeastieHunter.Services.Interfaces
{
    public interface IBTNotificationService
    {
        public Task AddNotificationAsync(Notification notification);

        public Task<List<Notification>> GetReceivedNotificationsAsync(string userId);

        public Task<List<Notification>> GetSentNotificationAsync(string userId);

        public Task SendEmailNotificationsByRoleAsync(Notification notification, int companyId, string role);

        public Task SendMembersEmailNotificationAsync(Notification notification, List<BTUser> members);

        public Task<bool> SendEmailNotificationsAsync(Notification notification, string emailSubject);
    }
}

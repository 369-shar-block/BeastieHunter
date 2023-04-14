using BeastieHunter.Models;

namespace BeastieHunter.Services.Interfaces
{
    public interface IBTNotificationService
    {

        public Task AddNotificationAsync(Notification notification);

        public Task AdminNotificationAsync(Notification notification, int companyId);

        public Task<List<Notification>> GetNotificationsByUserIdAsync(string userId);

        public Task<bool> SendAdminEmailNotificationAsync(Notification notification, string emailSubject, int companyId);

        public Task<bool> SendEmailNotificationAsync(Notification notification, string emailSubject);



        //public Task AddNotificationAsync(Notification notification);

        //public Task<List<Notification>> GetReceivedNotificationsAsync(string userId);

        //public Task<List<Notification>> GetSentNotificationAsync(string userId);

        //public Task SendEmailNotificationsByRoleAsync(Notification notification, int companyId, string role);

        //public Task SendMembersEmailNotificationAsync(Notification notification, List<BTUser> members);

        //public Task<bool> SendEmailNotificationsAsync(Notification notification, string emailSubject);
    }
}

using BeastieHunter.Data;
using BeastieHunter.Models;
using BeastieHunter.Models.Enums;
using BeastieHunter.Services.Interfaces;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;

namespace BeastieHunter.Services
{
    public class BTNotificationService : IBTNotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailSender _emailService;
        private readonly IBTRolesService _rolesService;

        public BTNotificationService(ApplicationDbContext context,
                                     IEmailSender emailService,
                                     IBTRolesService rolesService)
        {
            _context = context;
            _emailService = emailService;
            _rolesService = rolesService;
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            try
            {
                await _context.AddAsync(notification);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AdminNotificationAsync(Notification notification, int companyId)
        {
            try
            {
                IEnumerable<string> adminIds = (await _rolesService.GetUsersInRoleAsync(nameof(Roles.Admin), companyId)).Select(a => a.Id);


                foreach (string adminId in adminIds)
                {
                    notification.Id = 0;
                    notification.RecipientId = adminId;

                    await _context.AddAsync(notification);
                }

                await _context.SaveChangesAsync();

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Notification>> GetNotificationsByUserIdAsync(string userId)
        {
            try
            {
                List<Notification> notifications = new();

                notifications = await _context.Notifications
                                              .Where(n => n.SenderId == userId || n.RecipientId == userId)
                                              .Include(n => n.Recipient)
                                              .Include(n => n.Sender)
                                              .ToListAsync();
                return notifications;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> SendEmailNotificationAsync(Notification notification, string emailSubject)
        {
            try
            {
                BTUser? btUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == notification.RecipientId);

                string userEmail = btUser!.Email;

                await _emailService.SendEmailAsync(userEmail, emailSubject, notification.Message);
                return true;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> SendAdminEmailNotificationAsync(Notification notification, string emailSubject, int companyId)
        {
            try
            {
                IEnumerable<string> adminEmails = (await _rolesService.GetUsersInRoleAsync(nameof(Roles.Admin), companyId)).Select(a => a.Email);


                foreach (string adminEmail in adminEmails)
                {
                    await _emailService.SendEmailAsync(adminEmail, emailSubject, notification.Message);
                }

                return true;

            }
            catch (Exception)
            {

                throw;
            }
        }

        //    private readonly ApplicationDbContext _context;
        //    private readonly IEmailSender _emailSender;
        //    private readonly IBTRolesService _rolesService;

        //    public BTNotificationService(ApplicationDbContext context, IEmailSender emailSender, IBTRolesService rolesService)
        //    {
        //        _context = context;
        //        _emailSender = emailSender;
        //        _rolesService = rolesService;
        //    }


        //    public async Task AddNotificationAsync(Notification notification)
        //    {
        //        try
        //        {
        //            await _context.AddAsync(notification);
        //            await _context.SaveChangesAsync();

        //        }
        //        catch (Exception)
        //        { 

        //            throw;
        //        }
        //    }

        //    public async Task<List<Notification>> GetReceivedNotificationsAsync(string userId)
        //    {
        //        try
        //        {
        //            List<Notification> notifications = await _context.Notifications.Include(n => n.Recipient)
        //                                                                            .Include(n => n.Sender)
        //                                                                            .Include(n => n.Ticket)
        //                                                                            .ThenInclude(t => t.Project)
        //                                                                            .Where(n => n.RecipientId == userId).ToListAsync();

        //            return notifications;
        //        }
        //        catch (Exception)
        //        {

        //            throw;
        //        }
        //    }

        //    public async Task<List<Notification>> GetSentNotificationAsync(string userId)
        //    {
        //        try
        //        {
        //            List<Notification> notifications = await _context.Notifications.Include(n => n.Recipient)
        //                                                                            .Include(n => n.Sender)
        //                                                                            .Include(n => n.Ticket)
        //                                                                            .ThenInclude(t => t.Project)
        //                                                                            .Where(n => n.SenderId == userId).ToListAsync();

        //            return notifications;
        //        }
        //        catch (Exception)
        //        {

        //            throw;
        //        }
        //    }

        //    public async Task<bool> SendEmailNotificationsAsync(Notification notification, string emailSubject)
        //    {
        //        BTUser btUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == notification.RecipientId);

        //        if(btUser != null)
        //        {
        //            string btUserEmail = btUser.Email;
        //            string message = notification.Message;

        //            try
        //            {
        //                await _emailSender.SendEmailAsync(btUserEmail, emailSubject, message);
        //                return true;
        //            }
        //            catch (Exception)
        //            {

        //                throw;
        //            }
        //        }
        //        else
        //        {
        //            return false;
        //        }
        //    }

        //    public async Task SendEmailNotificationsByRoleAsync(Notification notification, int companyId, string role)
        //    {
        //        try
        //        {
        //            List<BTUser> members = await _rolesService.GetUsersInRoleAsync(role, companyId);

        //            foreach(BTUser btUser in members)
        //            {
        //                notification.RecipientId = btUser.Id;
        //                await SendEmailNotificationsAsync(notification, notification.Title);
        //            }
        //        }
        //        catch (Exception)
        //        {

        //            throw;
        //        }
        //    }

        //    public async Task SendMembersEmailNotificationAsync(Notification notification, List<BTUser> members)
        //    {
        //        try
        //        {
        //            foreach (BTUser btUser in members)
        //            {
        //                notification.RecipientId = btUser.Id;
        //                await SendEmailNotificationsAsync(notification, notification.Title);
        //            }
        //        }
        //        catch (Exception)
        //        {

        //            throw;
        //        }
        //    }
        //}
    }
}

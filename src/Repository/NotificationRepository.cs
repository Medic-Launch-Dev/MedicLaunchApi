using MedicLaunchApi.Data;
using MedicLaunchApi.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace MedicLaunchApi.Repository
{
    public class NotificationRepository
    {
        private readonly ApplicationDbContext context;

        public NotificationRepository(ApplicationDbContext context)
        {
            this.context = context;
        }

        public async Task CreateNotifications(CreateNotificationRequest request)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (request.UserIds == null || request.UserIds.Length == 0)
            {
                throw new InvalidOperationException("A notification must be associated with at least one user");
            }

            var notificationCreationList = new List<UserNotification>();
            foreach (var userId in request.UserIds)
            {
                notificationCreationList.Add(new UserNotification()
                {
                    Id = Guid.NewGuid().ToString(),
                    Message = request.Content,
                    UserId = userId,
                    Title = request.Title,
                    CreatedOn = DateTime.UtcNow
                });
            }

            await context.AddRangeAsync(notificationCreationList);
            await context.SaveChangesAsync();
        }

        public async Task<List<NotificationResponse>> GetNotificationsForUser(string userId)
        {
            ArgumentNullException.ThrowIfNull(userId);

            var notificationsForUser = await this.context.UserNotifications.Where(m => m.UserId == userId).ToListAsync();
            return notificationsForUser.Select(notification => new NotificationResponse()
            {
                Id = notification.Id,
                Message = notification.Message,
                CreatedOn = notification.CreatedOn,
                IsRead = notification.IsRead,
                Title = notification.Title
            }).ToList();
        }

        public async Task MarkNotificationAsRead(string userId, string notificationId)
        {
            if(userId == null || notificationId == null)
            {
                throw new InvalidOperationException("User and notification ids must be provided");
            }

            var notification = await this.context.UserNotifications.FirstOrDefaultAsync(m => m.UserId == userId && m.Id == notificationId);
            if (notification == null)
            {
                throw new InvalidOperationException("Notification not found");
            }

            notification.IsRead = true;
            notification.ReadOn = DateTime.UtcNow;
            await this.context.SaveChangesAsync();
        }
    }
}

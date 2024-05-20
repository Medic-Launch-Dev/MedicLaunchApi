using MedicLaunchApi.Data;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace MedicLaunchApi.Test
{
    [TestClass]
    public class UserNotificationTests
    {
        private NotificationRepository notificationRepository;
        private ApplicationDbContext context;

        [TestInitialize]
        public void Setup()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "MedicLaunchApi", new InMemoryDatabaseRoot())
                .Options;

            context = new ApplicationDbContext(options);
            notificationRepository = new NotificationRepository(context);
        }

        [TestMethod]
        public async Task CreateNotification_WithValidRequest_ShouldCreateNotification()
        {
            var request = new CreateNotificationRequest()
            {
                Content = "Test Notification",
                UserIds = new string[] { "1", "2" }
            };

            await notificationRepository.CreateNotifications(request);

            var notifications = await context.UserNotifications.ToListAsync();
            Assert.AreEqual(2, notifications.Count);
        }

        [TestMethod]
        public async Task CreateNotification_WithNoUserIds_ShouldThrowException()
        {
            var request = new CreateNotificationRequest()
            {
                Content = "Test Notification",
                UserIds = new string[] { }
            };

            await Assert.ThrowsExceptionAsync<InvalidOperationException>(() => notificationRepository.CreateNotifications(request));
        }

        [TestMethod]
        public async Task GetNotificationsForUser_WithValidUserId_ShouldReturnNotifications()
        {
            var userId = "1";
            var notificationRequest = new CreateNotificationRequest()
            {
                Content = "Test Notification",
                UserIds = new string[] { userId }
            };

            await notificationRepository.CreateNotifications(notificationRequest);

            var notifications = await notificationRepository.GetNotificationsForUser(userId);
            Assert.AreEqual(1, notifications.Count);
        }

        [TestMethod]
        public async Task GetNotificationsForUser_WithInvalidUserId_ShouldReturnEmptyList()
        {
            var notificationRequest = new CreateNotificationRequest()
            {
                Content = "Test Notification",
                UserIds = new string[] { "2" }
            };

            await notificationRepository.CreateNotifications(notificationRequest);

            var notifications = await notificationRepository.GetNotificationsForUser("1");
            Assert.AreEqual(0, notifications.Count);
        }

        [TestMethod]
        public async Task MarkNotificationAsRead_WithValidIds_ShouldMarkNotificationAsRead()
        {
            var userId = "1";
            var notification = new UserNotification()
            {
                Id = Guid.NewGuid().ToString(),
                Message = "Test Notification",
                UserId = userId,
                CreatedOn = DateTime.UtcNow
            };

            context.UserNotifications.Add(notification);
            await context.SaveChangesAsync();

            await notificationRepository.MarkNotificationAsRead(userId, notification.Id);

            var updatedNotifications = await notificationRepository.GetNotificationsForUser(userId);
            Assert.IsTrue(updatedNotifications.First().IsRead);
        }
    }
}

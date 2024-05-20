using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MedicLaunchApi.Controllers
{
    [Route("api/notification")]
    [ApiController]
    [Authorize]
    public class NotificationController : ControllerBase
    {
        private readonly NotificationRepository notificationRepository;
        private string CurrentUserId => User.FindFirst(ClaimTypes.NameIdentifier)!.Value;

        public NotificationController(NotificationRepository notificationRepository)
        {
            this.notificationRepository = notificationRepository;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateNotification(CreateNotificationRequest  request)
        {
            await notificationRepository.CreateNotifications(request);
            return Ok();
        }

        [HttpGet("user-notifications")]
        public async Task<IActionResult> GetNotificationsForUser()
        {
            var notifications = await notificationRepository.GetNotificationsForUser(CurrentUserId);
            return Ok(notifications);
        }

        [HttpPost("mark-as-read")]
        public async Task<IActionResult> MarkNotificationAsRead(string notificationId)
        {
            await notificationRepository.MarkNotificationAsRead(CurrentUserId, notificationId);
            return Ok();
        }
    }
}

namespace MedicLaunchApi.Models.ViewModels
{
    public class CreateNotificationRequest
    {
        public string[] UserIds { get; set; }

        public string Title { get; set; }

        public string Content { get; set; }
    }
}

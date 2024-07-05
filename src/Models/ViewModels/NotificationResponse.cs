namespace MedicLaunchApi.Models.ViewModels
{
    public class NotificationResponse
    {
        public string Id { get; set; }

        public string Title { get; set; }

        public string Message { get; set; }

        public DateTime CreatedOn { get; set; }

        public bool IsRead { get; internal set; }
    }
}

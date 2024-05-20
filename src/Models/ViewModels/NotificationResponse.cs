namespace MedicLaunchApi.Models.ViewModels
{
    public class NotificationResponse
    {
        public string Message { get; set; }

        public DateTime CreatedOn { get; set; }
        public bool IsRead { get; internal set; }
    }
}

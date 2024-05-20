namespace MedicLaunchApi.Models.ViewModels
{
    public class NotifyUserRequest
    {
        public string[] UserIdsToNotify { get; set; }

        public string Message { get; set; }
    }
}

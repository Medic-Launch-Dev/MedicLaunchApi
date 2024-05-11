namespace MedicLaunchApi.Models.ViewModels
{
    public class ResetUserPasswordRequest
    {
        public string UserId { get; set; }

        public string NewPassword { get; set; }
    }
}

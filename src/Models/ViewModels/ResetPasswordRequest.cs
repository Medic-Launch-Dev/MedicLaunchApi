namespace MedicLaunchApi.Models.ViewModels
{
    public class ResetPasswordRequest
    {
        public string CurrentPassword { get; set; }

        public string NewPassword { get; set; }
    }
}

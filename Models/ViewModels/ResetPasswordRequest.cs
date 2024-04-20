namespace MedicLaunchApi.Models.ViewModels
{
    public class ResetPasswordRequest
    {
        public string UserId { get; set; }

        public string NewPassword { get; set; }
    }
}

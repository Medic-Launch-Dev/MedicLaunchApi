namespace MedicLaunchApi.Models.ViewModels
{
    public class ResetPasswordRequestForStudent
    {
        public string CurrentPassword { get; set; }

        public string NewPassword { get; set; }
    }
}

namespace MedicLaunchApi.Models.ViewModels
{
    public class ResetUserPasswordRequestForAdmin
    {
        public string UserId { get; set; }

        public string NewPassword { get; set; }
    }
}

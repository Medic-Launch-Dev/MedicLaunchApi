using MedicLaunchApi.Models;

namespace MedicLaunchApi.Services
{
  public interface IMixPanelService
  {
    Task CreateUserProfile(MedicLaunchUser user, UserClientInfo userClientInfo);
    Task DeleteUserProfile(string userId);
  }
}
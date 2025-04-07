using MedicLaunchApi.Models;

namespace MedicLaunchApi.Services
{
  public interface IMixPanelService
  {
    Task CreateUserProfile(MedicLaunchUser user, string ipAddress);
  }
}
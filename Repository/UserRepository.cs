using MedicLaunchApi.Models;
using MedicLaunchApi.Storage;
using Microsoft.AspNetCore.Identity;

namespace MedicLaunchApi.Repository
{
    public class UserRepository
    {
        private readonly AzureBlobClient azureBlobClient;
        private readonly UserManager<MedicLaunchUser> userManager;
        public UserRepository(AzureBlobClient azureBlobClient, UserManager<MedicLaunchUser> userManager)
        {
            this.azureBlobClient = azureBlobClient;
            this.userManager = userManager;
        }

        public async Task<UserProfile> CreateUserProfile(UserProfile userProfile, CancellationToken cancellationToken)
        {
            var userProfileJsonPath = GetUserProfileJsonPath(userProfile.UserId);
            return await azureBlobClient.CreateItemAsync(userProfileJsonPath, userProfile, cancellationToken);
        }

        public async Task<UserProfile> GetUserProfile(string userId, CancellationToken cancellationToken)
        {
            var userProfileJsonPath = GetUserProfileJsonPath(userId);
            return await azureBlobClient.GetItemAsync<UserProfile>(userProfileJsonPath, cancellationToken, true);
        }

        public async Task UpdateUserProfile(UserProfile userProfile, CancellationToken cancellationToken)
        {
            var userProfileJsonPath = GetUserProfileJsonPath(userProfile.UserId);
            await azureBlobClient.UpdateItemAsync(userProfileJsonPath, userProfile, cancellationToken);
        }

        private string GetUserProfileJsonPath(string userId)
        {
            return $"users/{userId}/profile.json";
        }
    }
}

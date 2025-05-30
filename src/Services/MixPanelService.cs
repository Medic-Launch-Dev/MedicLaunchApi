using System.Text.Json;
using MedicLaunchApi.Models;

namespace MedicLaunchApi.Services
{
	public class MixPanelService : IMixPanelService
	{
		private readonly HttpClient _httpClient;
		private readonly string _token;

		public MixPanelService(IConfiguration configuration)
		{
			_token = Environment.GetEnvironmentVariable("MIXPANEL_PROJECT_TOKEN")
				?? throw new ArgumentNullException("MIXPANEL_PROJECT_TOKEN environment variable is not configured");
			_httpClient = new HttpClient
			{
				BaseAddress = new Uri("https://api.mixpanel.com")
			};
		}

		public async Task CreateUserProfile(MedicLaunchUser user, string ipAddress)
		{
			var dictionary = new Dictionary<string, object>[]
			{
				new Dictionary<string, object>
				{
					{ "$token", _token },
					{ "$distinct_id", user.Id },
					{ "$ip", ipAddress },
					{ "$set", new Dictionary<string, object>
						{
							{ "$email", user.Email },
							{ "$name", $"{user.FirstName} {user.LastName}" },
							{ "$created", DateTime.UtcNow }
						}
					}
				}
			};

			var response = await _httpClient.PostAsync(
				"/engage#profile-set",  // Remove ip=0 parameter
				new StringContent(
					JsonSerializer.Serialize(dictionary),
					System.Text.Encoding.UTF8,
					"application/json"
				)
			);

			response.EnsureSuccessStatusCode();
		}
	}
}
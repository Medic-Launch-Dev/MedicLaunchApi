using System.Diagnostics.Eventing.Reader;
using Azure;
using Azure.AI.OpenAI;
using OpenAI.Chat;
using System.Text.Json;

namespace MedicLaunchApi.Services
{
	public class AzureOpenAIService
	{
		private readonly string azureOpenAIEndpoint;
		private readonly string azureOpenAIKey;
		private readonly ILogger<AzureOpenAIService> logger;
		private readonly AzureOpenAIClient azureClient;
		private readonly string defaultModelName;

		public AzureOpenAIService(ILogger<AzureOpenAIService> logger, IConfiguration configuration)
		{
			this.logger = logger;

			azureOpenAIEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? string.Empty;
			if (string.IsNullOrEmpty(azureOpenAIEndpoint))
			{
				throw new Exception("AZURE_OPENAI_ENDPOINT environment variable is not set.");
			}

			azureOpenAIKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY") ?? string.Empty;
			if (string.IsNullOrEmpty(azureOpenAIKey))
			{
				throw new Exception("AZURE_OPENAI_KEY environment variable is not set.");
			}

			defaultModelName = "gpt-4o-mini";
			var credential = new AzureKeyCredential(azureOpenAIKey);
			azureClient = new AzureOpenAIClient(new Uri(azureOpenAIEndpoint), credential);
		}

		public async Task<string> GenerateChatCompletion(List<ChatMessage> messages, string? modelName = null, ChatCompletionOptions? options = null)
		{
			var chatClient = azureClient.GetChatClient(modelName ?? defaultModelName);

			try
			{
				ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);
				return completion.Content?.FirstOrDefault()?.Text ?? "{}";
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "Error generating response from OpenAI using model {ModelName}", modelName ?? defaultModelName);
				throw;
			}
		}
	}
}
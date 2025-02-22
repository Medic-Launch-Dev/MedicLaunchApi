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
		private readonly ChatClient chatClient;

		public AzureOpenAIService(ILogger<AzureOpenAIService> logger, IConfiguration configuration)
		{
			this.logger = logger;

			this.azureOpenAIEndpoint = Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT") ?? string.Empty;
			logger.LogInformation($"AZURE_OPENAI_ENDPOINT: {this.azureOpenAIEndpoint}");
			if (string.IsNullOrEmpty(this.azureOpenAIEndpoint))
			{
				throw new Exception("AZURE_OPENAI_ENDPOINT environment variable is not set.");
			}

			this.azureOpenAIKey = Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY") ?? string.Empty;
			if (string.IsNullOrEmpty(this.azureOpenAIKey))
			{
				throw new Exception("AZURE_OPENAI_KEY environment variable is not set.");
			}


			var credential = new AzureKeyCredential(this.azureOpenAIKey);
			var azureClient = new AzureOpenAIClient(new Uri(this.azureOpenAIEndpoint), credential);
			this.chatClient = azureClient.GetChatClient("gpt-4o-mini");
		}

		public async Task<string> GeneratePromptResponseAsync(List<ChatMessage> messages)
		{
			var options = new ChatCompletionOptions
			{
				Temperature = (float)0.7,
				MaxOutputTokenCount = 800,
				TopP = (float)0.95,
				ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
				FrequencyPenalty = 0,
				PresencePenalty = 0,
			};

			try
			{
				ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);

				if (completion.Content != null && completion.Content.Count > 0)
				{
					return completion.Content[0].Text;
				}
				else
				{
					return "{}";
				}
			}
			catch (Exception ex)
			{
				this.logger.LogError(ex, "Error generating response from OpenAI");
				throw;
			}
		}
	}
}

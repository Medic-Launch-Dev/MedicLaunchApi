using System.Text.Json;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using OpenAI.Chat;

namespace MedicLaunchApi.Services
{
	public class TextbookLessonGenerationService
	{
		private readonly AzureOpenAIService azureOpenAIService;
		private readonly TextbookLessonRepository textbookLessonRepository;

		public TextbookLessonGenerationService(
				AzureOpenAIService azureOpenAIService,
				TextbookLessonRepository textbookLessonRepository)
		{
			this.azureOpenAIService = azureOpenAIService;
			this.textbookLessonRepository = textbookLessonRepository;
		}

		private async Task<List<ChatMessage>> GenerateChatPrompt(string htmlContent)
		{
			var messages = new List<ChatMessage>();
			var promptFilesPath = "Resources/TextbookLessonGenerationPrompts/";

			var systemMessage = await File.ReadAllTextAsync(promptFilesPath + "SystemMessage.txt");
			messages.Add(new SystemChatMessage(systemMessage));
			var userMessage1 = await File.ReadAllTextAsync(promptFilesPath + "Example1UserMessage.txt");
			messages.Add(new UserChatMessage(userMessage1));
			var assistantMessage1 = await File.ReadAllTextAsync(promptFilesPath + "Example1AssistantMessage.txt");
			messages.Add(new AssistantChatMessage(assistantMessage1));
			var userMessage2 = await File.ReadAllTextAsync(promptFilesPath + "Example2UserMessage.txt");
			messages.Add(new UserChatMessage(userMessage2));
			var assistantMessage2 = await File.ReadAllTextAsync(promptFilesPath + "Example2AssistantMessage.txt");
			messages.Add(new AssistantChatMessage(assistantMessage2));

			messages.Add(new UserChatMessage(htmlContent));

			return messages;
		}

		public async Task<CreateTextbookLessonRequest> GenerateAndCreateTextbookLessonAsync(string htmlContent, string specialityId, string userId)
		{
			var messages = await GenerateChatPrompt(htmlContent);
			var options = new ChatCompletionOptions
			{
				Temperature = 0.7f,
				MaxOutputTokenCount = 800,
				TopP = 0.95f,
				ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
				FrequencyPenalty = 0,
				PresencePenalty = 0,
			};
			var response = await azureOpenAIService.GenerateChatCompletion(messages: messages, options: options);

			JsonSerializerOptions jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
			var createTextbookLessonRequest = JsonSerializer.Deserialize<CreateTextbookLessonRequest>(response, jsonOptions);
			createTextbookLessonRequest.SpecialityId = specialityId;
			// var textbookLessonId = await textbookLessonRepository.CreateTextbookLessonAsync(createTextbookLessonRequest, userId);

			return createTextbookLessonRequest;
		}
	}
}

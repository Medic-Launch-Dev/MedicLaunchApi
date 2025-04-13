using System.Text.Json;
using MedicLaunchApi.Models.ViewModels;
using MedicLaunchApi.Repository;
using MedicLaunchApi.Models.OpenAI;

namespace MedicLaunchApi.Services
{
	public class TextbookLessonGenerationService
	{
		private readonly OpenAIService openAIService;

		public TextbookLessonGenerationService(OpenAIService openAIService)
		{
			this.openAIService = openAIService;
		}

		private async Task<List<ChatMessage>> GenerateChatPrompt(string htmlContent)
		{
			var messages = new List<ChatMessage>();
			var promptFilesPath = "Resources/TextbookLessonGenerationPrompts/";

			var systemUserMessage = await File.ReadAllTextAsync(promptFilesPath + "SystemMessage.txt");
			messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = systemUserMessage }] });
			var example1UserMessage = await File.ReadAllTextAsync(promptFilesPath + "Example1UserMessage.txt");
			messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = example1UserMessage }] });
			var example1AssistantMessage = await File.ReadAllTextAsync(promptFilesPath + "Example1AssistantMessage.txt");
			messages.Add(new ChatMessage { Role = "assistant", Content = [new ChatContent { Text = example1AssistantMessage }] });
			var example2UserMessage = await File.ReadAllTextAsync(promptFilesPath + "Example2UserMessage.txt");
			messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = example2UserMessage }] });
			var example2AssistantMessage = await File.ReadAllTextAsync(promptFilesPath + "Example2AssistantMessage.txt");
			messages.Add(new ChatMessage { Role = "assistant", Content = [new ChatContent { Text = example2AssistantMessage }] });

			messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = htmlContent }] });

			return messages;
		}

		public async Task<CreateTextbookLessonRequest> GenerateTextbookLessonAsync(string htmlContent, string specialityId, string? questionId)
		{
			var messages = await GenerateChatPrompt(htmlContent);
			var response = await openAIService.GenerateChatCompletion(messages: messages, modelName: "gpt-4o");

			JsonSerializerOptions jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
			var createTextbookLessonRequest = JsonSerializer.Deserialize<CreateTextbookLessonRequest>(response, jsonOptions);

			if (createTextbookLessonRequest == null)
			{
				throw new Exception("Failed to deserialize the response from OpenAI.");
			}

			createTextbookLessonRequest.SpecialityId = specialityId;
			createTextbookLessonRequest.QuestionId = questionId;
			// createTextbookLessonRequest.SpecialityId = specialityId;
			// var textbookLessonId = await textbookLessonRepository.CreateTextbookLessonAsync(createTextbookLessonRequest, userId);

			return createTextbookLessonRequest;
		}
	}
}

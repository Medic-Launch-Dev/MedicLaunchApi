using System.Text.Json;
using MedicLaunchApi.Models.OpenAI;
using MedicLaunchApi.Models.QuestionDTOs;

namespace MedicLaunchApi.Services
{
  public class QuestionGenerationService : IQuestionGenerationService
  {
    private readonly OpenAIService openAIService;

    public QuestionGenerationService(OpenAIService openAIService)
    {
      this.openAIService = openAIService;
    }

    private async Task<List<ChatMessage>> GenerateChatPrompt(string conditions)
    {
      var messages = new List<ChatMessage>();
      var promptFilesPath = "Resources/QuestionGenerationPrompts/QuestionAndExplanation/";

      var systemUserMessage = await File.ReadAllTextAsync(promptFilesPath + "SystemUserMessage.txt");
      messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = systemUserMessage }] });
      var example1UserMessage = await File.ReadAllTextAsync(promptFilesPath + "Example1UserMessage.txt");
      messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = example1UserMessage }] });
      var example1AssistantMessage = await File.ReadAllTextAsync(promptFilesPath + "Example1AssistantMessage.txt");
      messages.Add(new ChatMessage { Role = "assistant", Content = [new ChatContent { Text = example1AssistantMessage }] });
      var example2UserMessage = await File.ReadAllTextAsync(promptFilesPath + "Example2UserMessage.txt");
      messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = example2UserMessage }] });
      var example2AssistantMessage = await File.ReadAllTextAsync(promptFilesPath + "Example2AssistantMessage.txt");
      messages.Add(new ChatMessage { Role = "assistant", Content = [new ChatContent { Text = example2AssistantMessage }] });
      var example3UserMessage = await File.ReadAllTextAsync(promptFilesPath + "Example3UserMessage.txt");
      messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = example3UserMessage }] });
      var example3AssistantMessage = await File.ReadAllTextAsync(promptFilesPath + "Example3AssistantMessage.txt");
      messages.Add(new ChatMessage { Role = "assistant", Content = [new ChatContent { Text = example3AssistantMessage }] });

      var content = $"Turn the following into an MCQ: {conditions}";
      messages.Add(new ChatMessage { Role = "user", Content = [new ChatContent { Text = content }] });

      return messages;
    }

    public async Task<QuestionTextAndExplanation> GenerateQuestionTextAndExplanationAsync(string content)
    {
      var messages = await GenerateChatPrompt(content);

      var response = await openAIService.GenerateChatCompletion(messages);

      JsonSerializerOptions jsonOptions = new()
      {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
      };

      return JsonSerializer.Deserialize<QuestionTextAndExplanation>(response, jsonOptions);
    }
  }
}
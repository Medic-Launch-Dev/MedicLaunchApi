using System.Net.Http.Headers;
using System.Text.Json;
using MedicLaunchApi.Models.OpenAI;

public class OpenAIService
{
  private readonly HttpClient httpClient;
  private readonly ILogger<OpenAIService> logger;
  private readonly string openAIKey;
  private readonly string defaultModelName;

  public OpenAIService(ILogger<OpenAIService> logger)
  {
    this.logger = logger;

    openAIKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY") ?? string.Empty;
    if (string.IsNullOrEmpty(openAIKey))
    {
      throw new Exception("OPENAI_API_KEY environment variable is not set.");
    }

    defaultModelName = "o1";
    httpClient = new HttpClient();
    httpClient.BaseAddress = new Uri("https://api.openai.com/v1/");
    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", openAIKey);
  }

  public async Task<string> GenerateChatCompletion(List<ChatMessage> messages, string? modelName = null)
  {
    var requestBody = new
    {
      model = modelName ?? defaultModelName,
      messages = messages.Select(m => new
      {
        role = m.Role.ToString().ToLower(),
        content = m.Content.Select(c => new
        {
          text = c.Text,
          type = "text"
        }).ToArray()
      }).ToArray(),
      store = true
    };

    try
    {
      var response = await httpClient.PostAsync(
          "chat/completions",
          new StringContent(
              JsonSerializer.Serialize(requestBody),
              MediaTypeHeaderValue.Parse("application/json")
          )
      );

      response.EnsureSuccessStatusCode();
      var jsonResponse = await response.Content.ReadAsStringAsync();

      // Parse response to extract just the message content
      using var doc = JsonDocument.Parse(jsonResponse);
      var choices = doc.RootElement.GetProperty("choices");
      if (choices.GetArrayLength() > 0)
      {
        var firstChoice = choices[0];
        var message = firstChoice.GetProperty("message");
        var content = message.GetProperty("content");
        var chatCompletion = content.GetString();
        return chatCompletion;
      }

      return "{}";
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error generating response from OpenAI using model {ModelName}", modelName ?? defaultModelName);
      throw;
    }
  }
}
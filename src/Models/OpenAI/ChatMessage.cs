namespace MedicLaunchApi.Models.OpenAI
{
  public class ChatMessage
  {
    public string Role { get; set; }
    public ChatContent[] Content { get; set; }
  }
}
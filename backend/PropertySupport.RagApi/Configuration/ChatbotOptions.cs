namespace PropertySupport.RagApi.Configuration;

public sealed class ChatbotOptions
{
    public const string SectionName = "Chatbot";
    public string SystemPrompt { get; set; } = string.Empty;
    public int MaxHistoryMessages { get; set; } = 6;
    public int MaxContextCharacters { get; set; } = 12000;
}

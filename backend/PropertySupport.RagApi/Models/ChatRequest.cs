namespace PropertySupport.RagApi.Models;

public sealed class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public List<ChatMessageDto> History { get; set; } = [];
}

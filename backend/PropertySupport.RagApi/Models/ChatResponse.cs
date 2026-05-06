namespace PropertySupport.RagApi.Models;

public sealed class ChatResponse
{
    public string Reply { get; set; } = string.Empty;
    public List<SourceReference> Sources { get; set; } = [];
}

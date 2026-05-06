namespace PropertySupport.RagApi.Models;

public sealed class KnowledgeChunk
{
    public string Content { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public double? Score { get; set; }
}

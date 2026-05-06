namespace PropertySupport.RagApi.Models;

public sealed class SourceReference
{
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public double? Score { get; set; }
}

namespace PropertySupport.RagApi.Configuration;

public sealed class AzureAiSearchOptions
{
    public const string SectionName = "AzureAiSearch";
    public string Endpoint { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public string IndexName { get; set; } = string.Empty;
    public string ContentFieldName { get; set; } = "content";
    public string TitleFieldName { get; set; } = "metadata_storage_name";
    public string UrlFieldName { get; set; } = "metadata_storage_path";
    public int Top { get; set; } = 5;
    public bool UseSemanticSearch { get; set; }
    public string SemanticConfigurationName { get; set; } = string.Empty;
}

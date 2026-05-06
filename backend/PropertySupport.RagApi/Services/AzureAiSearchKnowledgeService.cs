using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using Microsoft.Extensions.Options;
using PropertySupport.RagApi.Configuration;
using PropertySupport.RagApi.Models;

namespace PropertySupport.RagApi.Services;

public sealed class AzureAiSearchKnowledgeService : IKnowledgeSearchService
{
    private readonly SearchClient _searchClient;
    private readonly AzureAiSearchOptions _options;
    private readonly ILogger<AzureAiSearchKnowledgeService> _logger;

    public AzureAiSearchKnowledgeService(
        IOptions<AzureAiSearchOptions> options,
        ILogger<AzureAiSearchKnowledgeService> logger)
    {
        _options = options.Value;
        _logger = logger;
        ValidateConfiguration();

        _searchClient = new SearchClient(
            new Uri(_options.Endpoint),
            _options.IndexName,
            new AzureKeyCredential(_options.ApiKey));
    }

    public async Task<IReadOnlyList<KnowledgeChunk>> SearchAsync(string question, CancellationToken cancellationToken)
    {
        var searchOptions = new SearchOptions
        {
            Size = _options.Top,
            IncludeTotalCount = false
        };

        AddSelectField(searchOptions, _options.ContentFieldName);
        AddSelectField(searchOptions, _options.TitleFieldName);
        AddSelectField(searchOptions, _options.UrlFieldName);

        if (_options.UseSemanticSearch)
        {
            searchOptions.QueryType = SearchQueryType.Semantic;
            searchOptions.SemanticSearch = new SemanticSearchOptions();

            if (!string.IsNullOrWhiteSpace(_options.SemanticConfigurationName))
                searchOptions.SemanticSearch.SemanticConfigurationName = _options.SemanticConfigurationName;
        }

        var chunks = new List<KnowledgeChunk>();

        try
        {
            var response = await _searchClient.SearchAsync<SearchDocument>(
                question,
                searchOptions,
                cancellationToken);

            await foreach (var result in response.Value.GetResultsAsync())
            {
                var document = result.Document;
                var content = GetStringField(document, _options.ContentFieldName);

                if (string.IsNullOrWhiteSpace(content))
                    continue;

                chunks.Add(new KnowledgeChunk
                {
                    Content = content,
                    Title = GetStringField(document, _options.TitleFieldName),
                    Url = GetStringField(document, _options.UrlFieldName),
                    Score = result.Score
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Azure AI Search query failed.");
            throw;
        }

        return chunks;
    }

    private static void AddSelectField(SearchOptions options, string fieldName)
    {
        if (!string.IsNullOrWhiteSpace(fieldName) && !options.Select.Contains(fieldName))
            options.Select.Add(fieldName);
    }

    private static string GetStringField(SearchDocument document, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(fieldName))
            return string.Empty;

        if (!document.TryGetValue(fieldName, out var value) || value is null)
            return string.Empty;

        return value switch
        {
            string s => s,
            IEnumerable<string> values => string.Join(", ", values),
            _ => value.ToString() ?? string.Empty
        };
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_options.Endpoint))
            throw new InvalidOperationException("AzureAiSearch:Endpoint is missing.");

        if (string.IsNullOrWhiteSpace(_options.ApiKey))
            throw new InvalidOperationException("AzureAiSearch:ApiKey is missing.");

        if (string.IsNullOrWhiteSpace(_options.IndexName))
            throw new InvalidOperationException("AzureAiSearch:IndexName is missing.");

        if (string.IsNullOrWhiteSpace(_options.ContentFieldName))
            throw new InvalidOperationException("AzureAiSearch:ContentFieldName is missing.");
    }
}

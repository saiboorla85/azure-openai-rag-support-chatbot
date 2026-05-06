using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using PropertySupport.RagApi.Configuration;
using PropertySupport.RagApi.Models;
using Microsoft.ApplicationInsights;
using System.Text;

namespace PropertySupport.RagApi.Services;

public sealed class ChatService : IChatService
{
    private readonly Kernel _kernel;
    private readonly IKnowledgeSearchService _knowledgeSearchService;
    private readonly ChatbotOptions _chatbotOptions;
    private readonly ILogger<ChatService> _logger;
    private readonly TelemetryClient _telemetryClient;

    public ChatService(
        Kernel kernel,
        IKnowledgeSearchService knowledgeSearchService,
        IOptions<ChatbotOptions> chatbotOptions,
        ILogger<ChatService> logger,
        TelemetryClient telemetryClient)
    {
        _kernel = kernel;
        _knowledgeSearchService = knowledgeSearchService;
        _chatbotOptions = chatbotOptions.Value;
        _logger = logger;
        _telemetryClient = telemetryClient;

    }

    public async Task<ChatResponse> GetReplyAsync(ChatRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Message))
            return new ChatResponse { Reply = "Please enter a question." };

        if (request.Message.Length > 2000)
            return new ChatResponse { Reply = "Your question is too long. Please shorten it and try again." };

        try
        {
            var knowledgeChunks = await _knowledgeSearchService.SearchAsync(request.Message, cancellationToken);
            var bestScore = knowledgeChunks
           .Where(x => x.Score.HasValue)
           .Select(x => x.Score!.Value)
           .DefaultIfEmpty(0)
           .Max();

            if (knowledgeChunks.Count == 0 || bestScore < 0.5)
            {
                TrackUnansweredQuestion(
                    request.Message,
                    knowledgeChunks.Count == 0 ? "NoSearchResults" : "LowSearchScore",
        bestScore);

                return new ChatResponse
                {
                    Reply = "I could not find this in the support documentation. Please contact support."
                };
            }

            var chatHistory = BuildChatHistory(request, knowledgeChunks);
            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

            var result = await chatCompletionService.GetChatMessageContentAsync(
                chatHistory: chatHistory,
                kernel: _kernel,
                cancellationToken: cancellationToken);

            return new ChatResponse
            {
                Reply = result.Content ?? "Sorry, I could not generate an answer.",
                Sources = knowledgeChunks.Select(x => new SourceReference
                {
                    Title = x.Title,
                    Url = x.Url,
                    Score = x.Score
                }).ToList()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate RAG chatbot response.");
            return new ChatResponse
            {
                Reply = "Sorry, I could not connect to the support knowledge base or AI service. Please check the backend logs and Azure configuration."
            };
        }
    }

    private ChatHistory BuildChatHistory(ChatRequest request, IReadOnlyList<KnowledgeChunk> chunks)
    {
        var chatHistory = new ChatHistory();
        chatHistory.AddSystemMessage(_chatbotOptions.SystemPrompt);

        foreach (var message in request.History.TakeLast(_chatbotOptions.MaxHistoryMessages))
        {
            if (string.IsNullOrWhiteSpace(message.Content))
                continue;

            if (message.Role.Equals("user", StringComparison.OrdinalIgnoreCase))
                chatHistory.AddUserMessage(message.Content);
            else if (message.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase) ||
                     message.Role.Equals("bot", StringComparison.OrdinalIgnoreCase))
                chatHistory.AddAssistantMessage(message.Content);
        }

        var context = BuildContextText(chunks);

        chatHistory.AddUserMessage($"""
        Support documentation context:
        {context}

        Customer question:
        {request.Message}

        Answer instructions:
        - Answer using only the support documentation context.
        - Keep the answer short and effective.
        - Use maximum 5 bullet points.
        - Explain the likely cause.
        - Give clear steps to fix the issue.
        - If the context does not contain the answer, say you could not find it in the support documentation.
        """);

        return chatHistory;
    }

    private string BuildContextText(IReadOnlyList<KnowledgeChunk> chunks)
    {
        var builder = new StringBuilder();

        foreach (var chunk in chunks)
        {
            builder.AppendLine("SOURCE:");
            builder.AppendLine(string.IsNullOrWhiteSpace(chunk.Title) ? "Untitled support document" : chunk.Title);
            builder.AppendLine("CONTENT:");
            builder.AppendLine(chunk.Content);
            builder.AppendLine();
            builder.AppendLine("---");
            builder.AppendLine();

            if (builder.Length >= _chatbotOptions.MaxContextCharacters)
                break;
        }

        var context = builder.ToString();

        return context.Length <= _chatbotOptions.MaxContextCharacters
            ? context
            : context[.._chatbotOptions.MaxContextCharacters];
    }

    private void TrackUnansweredQuestion(string question, string reason, double bestScore)
    {
        var properties = new Dictionary<string, string>
        {
            ["Question"] = question,
            ["Reason"] = reason,
            ["BestSearchScore"] = bestScore.ToString("0.000"),
            ["TimestampUtc"] = DateTime.UtcNow.ToString("O")
        };

        _telemetryClient.TrackEvent("UnansweredQuestion", properties);
    }
}

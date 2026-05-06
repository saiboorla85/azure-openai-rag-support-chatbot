using PropertySupport.RagApi.Models;

namespace PropertySupport.RagApi.Services;

public interface IChatService
{
    Task<ChatResponse> GetReplyAsync(ChatRequest request, CancellationToken cancellationToken);
}

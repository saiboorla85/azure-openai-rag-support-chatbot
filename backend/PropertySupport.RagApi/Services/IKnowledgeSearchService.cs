using PropertySupport.RagApi.Models;

namespace PropertySupport.RagApi.Services;

public interface IKnowledgeSearchService
{
    Task<IReadOnlyList<KnowledgeChunk>> SearchAsync(string question, CancellationToken cancellationToken);
}

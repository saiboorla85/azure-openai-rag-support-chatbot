using Microsoft.AspNetCore.Mvc;
using PropertySupport.RagApi.Services;

namespace PropertySupport.RagApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class KnowledgeController : ControllerBase
{
    private readonly IKnowledgeSearchService _knowledgeSearchService;

    public KnowledgeController(IKnowledgeSearchService knowledgeSearchService)
    {
        _knowledgeSearchService = knowledgeSearchService;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string question, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(question))
            return BadRequest(new { error = "Question is required." });

        return Ok(await _knowledgeSearchService.SearchAsync(question, cancellationToken));
    }
}

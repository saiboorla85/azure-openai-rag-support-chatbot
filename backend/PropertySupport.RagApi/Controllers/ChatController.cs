using Microsoft.AspNetCore.Mvc;
using PropertySupport.RagApi.Models;
using PropertySupport.RagApi.Services;
using Microsoft.AspNetCore.Authorization;

namespace PropertySupport.RagApi.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ChatResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ChatResponse>> Post([FromBody] ChatRequest request, CancellationToken cancellationToken)
    {
        if (request is null)
            return BadRequest(new ErrorResponse { Error = "Request body is required." });

        return Ok(await _chatService.GetReplyAsync(request, cancellationToken));
    }
}

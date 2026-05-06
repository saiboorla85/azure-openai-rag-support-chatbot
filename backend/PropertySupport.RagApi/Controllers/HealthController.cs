using Microsoft.AspNetCore.Mvc;

namespace PropertySupport.RagApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(new
        {
            status = "Healthy",
            application = "PropertySupport.RagApi",
            timestampUtc = DateTime.UtcNow
        });
    }
}

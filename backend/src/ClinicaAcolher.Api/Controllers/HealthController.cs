using Microsoft.AspNetCore.Mvc;

namespace ClinicaAcolher.Api.Controllers;

public sealed record HealthResponse(string Status, string Service, DateTimeOffset Timestamp);

[ApiController]
[Route("api/health")]
public sealed class HealthController(TimeProvider clock) : ControllerBase
{
    [HttpGet]
    public ActionResult<HealthResponse> Get() => 
    Ok(new HealthResponse("ok", "ClinicaAcolher.Api", clock.GetUtcNow()));
}
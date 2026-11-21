using ClinicaAcolher.Application.PatientAccess;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace ClinicaAcolher.Api.Controllers;

[ApiController]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class PatientInvitationsController(PatientInvitationService service) : ControllerBase
{
    [HttpPost("/api/v1/patients/{patientId:guid}/invitations")]
    public async Task<ActionResult<InvitationIssuedResponse>> Issue(Guid patientId, CancellationToken ct) =>
        StatusCode(StatusCodes.Status201Created, await service.IssueAsync(patientId, ct));

    [AllowAnonymous]
    [EnableRateLimiting("convites")]
    [HttpPost("/api/v1/patient-invitations/accept")]
    public async Task<IActionResult> Accept(AcceptInvitationRequest request, CancellationToken ct)
    {
        await service.AcceptAsync(request, ct);
        return NoContent();
    }
}
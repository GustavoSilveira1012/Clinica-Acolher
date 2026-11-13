using ClinicaAcolher.Application.Attendance;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaAcolher.Api.Controllers;

[ApiController]
public sealed class AttendancePoliciesController(AttendancePolicyService service) : ControllerBase
{
    [HttpPost("/api/v1/professionals/{professionalId:guid}/attendance-policies")]
    public async Task<ActionResult<AttendancePolicyResponse>> Publish(
        Guid professionalId, PublishAttendancePolicyRequest request, CancellationToken ct)
    {
        var policy = await service.PublishAsync(professionalId, request, ct);
        return CreatedAtAction(nameof(Current), new { professionalId }, policy);
    }

    [HttpGet("/api/v1/professionals/{professionalId:guid}/attendance-policies/current")]
    public async Task<ActionResult<AttendancePolicyResponse>> Current(Guid professionalId, CancellationToken ct) =>
        Ok(await service.GetCurrentAsync(professionalId, ct));

    [HttpPost("/api/v1/attendance-policies/{policyId:guid}/acceptances")]
    public async Task<ActionResult<PolicyAcceptanceResponse>> Accept(
        Guid policyId, AcceptPolicyRequest request, CancellationToken ct) =>
        Ok(await service.AcceptAsync(policyId, request, ct));
}
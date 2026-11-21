using ClinicaAcolher.Api.Auth;
using ClinicaAcolher.Application.Portal;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaAcolher.Api.Controllers;

[ApiController]
// Nome qualificado: a ação Policies() esconderia a classe Auth.Policies aqui dentro
[Authorize(Policy = ClinicaAcolher.Api.Auth.Policies.Patient)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class PortalController(PortalService service) : ControllerBase
{
    [HttpGet("/api/v1/portal/appointments")]
    public async Task<ActionResult<PortalAgendaResponse>> Agenda(CancellationToken ct) =>
        Ok(await service.GetAgendaAsync(ct));

    [HttpGet("/api/v1/portal/attendance-policies")]
    public async Task<ActionResult<IReadOnlyList<PortalPolicyResponse>>> Policies(CancellationToken ct) =>
        Ok(await service.ListPoliciesAsync(ct));

    [HttpPost("/api/v1/portal/attendance-policies/{policyId:guid}/accept")]
    public async Task<ActionResult<PortalPolicyResponse>> AcceptPolicy(Guid policyId, CancellationToken ct) =>
        Ok(await service.AcceptPolicyAsync(policyId, ct));
}
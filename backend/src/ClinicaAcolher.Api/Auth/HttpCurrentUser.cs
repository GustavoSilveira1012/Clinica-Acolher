using System.Security.Claims;
using ClinicaAcolher.Application.Common;
using ClinicaAcolher.Infrastructure.Identity;

namespace ClinicaAcolher.Api.Auth;

public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;

    public bool IsClinicAdmin => Principal?.IsInRole(Roles.Admin) == true;
    public Guid? ProfessionalId => ReadGuid(AppClaims.ProfessionalId);
    public Guid? PatientId => ReadGuid(AppClaims.PatientId);

    private Guid? ReadGuid(string claimType) =>
        Guid.TryParse(Principal?.FindFirstValue(claimType), out var id) ? id : null;
}
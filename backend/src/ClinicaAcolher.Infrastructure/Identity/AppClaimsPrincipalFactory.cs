using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace ClinicaAcolher.Infrastructure.Identity;

public sealed class AppClaimsPrincipalFactory(
    UserManager<ApplicationUser> users, RoleManager<IdentityRole<Guid>> roles, IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<ApplicationUser, IdentityRole<Guid>>(users, roles, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        if (user.ProfessionalId is { } professionalId)
            identity.AddClaim(new Claim(AppClaims.ProfessionalId, professionalId.ToString()));
        if (user.PatientId is { } patientId)
            identity.AddClaim(new Claim(AppClaims.PatientId, patientId.ToString()));

        return identity;
    }
}
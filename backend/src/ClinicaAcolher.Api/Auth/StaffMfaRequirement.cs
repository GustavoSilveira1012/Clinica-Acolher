using System.Security.Claims;
using ClinicaAcolher.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;

namespace ClinicaAcolher.Api.Auth;

public static class Policies
{
    /// <summary>Só exige login, sem exigir 2FA. Para as telas de configurar o 2FA, "quem sou eu" e sair.</summary>
    public const string LoggedIn = "LoggedIn";
}

//RN-SEG-03 e RN-PRO-07: a equipe só acessa dados depois de passar pelo segundo fator
public sealed class StaffMfaRequirement : IAuthorizationRequirement;

public sealed class StaffMfaHandler : AuthorizationHandler<StaffMfaRequirement>
{
    public static bool IsStaff(ClaimsPrincipal user) =>
        user.IsInRole(Roles.Admin) || user.IsInRole(Roles.Professional);

    public static bool PassedMfa(ClaimsPrincipal user) => user.HasClaim("amr", "mfa");

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, StaffMfaRequirement requirement)
    {
        if (!IsStaff(context.User) || PassedMfa(context.User))
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
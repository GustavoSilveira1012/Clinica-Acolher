using ClinicaAcolher.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaAcolher.Api.Controllers;

public sealed record LoginRequest(string Email, string Password);

public sealed record CurrentUserResponse(
    Guid Id, string Email, IReadOnlyList<string> Roles, Guid? ProfessionalId, Guid? PatientId);

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users)
    : ControllerBase
{
    private const string GenericFailure =
        "E-mail ou senha incorretos, ou acesso bloqueado temporariamente por muitas tentativas.";

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<CurrentUserResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var result = await signIn.PasswordSignInAsync(email, request.Password, isPersistent: false, lockoutOnFailure: true);

        if (!result.Succeeded)
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Não foi possível entrar", detail: GenericFailure);

        var user = await users.FindByNameAsync(email);
        return Ok(await ToResponseAsync(user!));
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();
        return NoContent();
    }

    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserResponse>> Me()
    {
        var user = await users.GetUserAsync(User);
        return user is null ? Unauthorized() : Ok(await ToResponseAsync(user));
    }

    private async Task<CurrentUserResponse> ToResponseAsync(ApplicationUser user) =>
        new(user.Id, user.Email!, (await users.GetRolesAsync(user)).ToList(), user.ProfessionalId, user.PatientId);
}
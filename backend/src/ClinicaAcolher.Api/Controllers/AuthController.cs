using System.Security.Claims;
using ClinicaAcolher.Api.Auth;
using ClinicaAcolher.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QRCoder;

namespace ClinicaAcolher.Api.Controllers;

public sealed record LoginRequest(string Email, string Password);
public sealed record TwoFactorCodeRequest(string Code);

public sealed record CurrentUserResponse(
    Guid Id, string Email, IReadOnlyList<string> Roles, Guid? ProfessionalId, Guid? PatientId,
    bool TwoFactorEnabled, bool MustSetupTwoFactor);

public sealed record LoginResponse(bool RequiresTwoFactor, CurrentUserResponse? User);
public sealed record TwoFactorSetupResponse(string ManualKey, string QrCodeDataUri);
public sealed record RecoveryCodesResponse(IReadOnlyList<string> RecoveryCodes);

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthController(SignInManager<ApplicationUser> signIn, UserManager<ApplicationUser> users)
    : ControllerBase
{
    private const string Issuer = "Clinica Acolher";
    private const string GenericFailure =
        "E-mail, senha ou código incorretos, ou acesso bloqueado temporariamente por muitas tentativas.";

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var result = await signIn.PasswordSignInAsync(email, request.Password, isPersistent: false, lockoutOnFailure: true);

        if (result.RequiresTwoFactor)
            return Ok(new LoginResponse(RequiresTwoFactor: true, User: null));
        if (!result.Succeeded)
            return Failure();

        var user = await users.FindByNameAsync(email);
        return Ok(new LoginResponse(false, await ToResponseAsync(user!)));
    }

    [AllowAnonymous]
    [HttpPost("login/2fa")]
    public async Task<ActionResult<LoginResponse>> LoginWithCode(TwoFactorCodeRequest request)
    {
        var user = await signIn.GetTwoFactorAuthenticationUserAsync();
        var result = await signIn.TwoFactorAuthenticatorSignInAsync(
            NormalizeTotp(request.Code), isPersistent: false, rememberClient: false);

        return user is not null && result.Succeeded
            ? Ok(new LoginResponse(false, await ToResponseAsync(user)))
            : Failure();
    }

    [AllowAnonymous]
    [HttpPost("login/recovery")]
    public async Task<ActionResult<LoginResponse>> LoginWithRecoveryCode(TwoFactorCodeRequest request)
    {
        var user = await signIn.GetTwoFactorAuthenticationUserAsync();
        var result = await signIn.TwoFactorRecoveryCodeSignInAsync(request.Code.Trim());

        return user is not null && result.Succeeded
            ? Ok(new LoginResponse(false, await ToResponseAsync(user)))
            : Failure();
    }

    [Authorize(Policy = Policies.LoggedIn)]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();
        return NoContent();
    }

    [Authorize(Policy = Policies.LoggedIn)]
    [HttpGet("me")]
    public async Task<ActionResult<CurrentUserResponse>> Me()
    {
        var user = await users.GetUserAsync(User);
        return user is null ? Unauthorized() : Ok(await ToResponseAsync(user));
    }

    [Authorize(Policy = Policies.LoggedIn)]
    [HttpPost("2fa/setup")]
    public async Task<ActionResult<TwoFactorSetupResponse>> SetupTwoFactor()
    {
        var user = await users.GetUserAsync(User);
        if (user is null)
            return Unauthorized();
        if (user.TwoFactorEnabled)
            return Problem(statusCode: StatusCodes.Status409Conflict, title: "Dois fatores já ativos",
                detail: "A autenticação em dois fatores já está ativa nesta conta.");

        await users.ResetAuthenticatorKeyAsync(user);
        var key = (await users.GetAuthenticatorKeyAsync(user))!;

        var uri = $"otpauth://totp/{Uri.EscapeDataString(Issuer)}:{Uri.EscapeDataString(user.Email!)}" +
                  $"?secret={key}&issuer={Uri.EscapeDataString(Issuer)}&digits=6";

        using var generator = new QRCodeGenerator();
        var png = new PngByteQRCode(generator.CreateQrCode(uri, QRCodeGenerator.ECCLevel.Q)).GetGraphic(6);

        return Ok(new TwoFactorSetupResponse(FormatKey(key), "data:image/png;base64," + Convert.ToBase64String(png)));
    }

    [Authorize(Policy = Policies.LoggedIn)]
    [HttpPost("2fa/enable")]
    public async Task<ActionResult<RecoveryCodesResponse>> EnableTwoFactor(TwoFactorCodeRequest request)
    {
        var user = await users.GetUserAsync(User);
        if (user is null)
            return Unauthorized();

        var valid = await users.VerifyTwoFactorTokenAsync(
            user, users.Options.Tokens.AuthenticatorTokenProvider, NormalizeTotp(request.Code));
        if (!valid)
            return Problem(statusCode: StatusCodes.Status422UnprocessableEntity, title: "Código inválido",
                detail: "O código não confere. Confira se o horário do celular está automático e use o código que aparece agora.");

        await users.SetTwoFactorEnabledAsync(user, true);
        var codes = await users.GenerateNewTwoFactorRecoveryCodesAsync(user, 10);

        // A sessão atual passa a contar como verificada em dois fatores
        await signIn.SignInWithClaimsAsync(user, isPersistent: false, [new Claim("amr", "mfa")]);

        return Ok(new RecoveryCodesResponse(codes!.ToList()));
    }

    private ActionResult Failure() =>
        Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Não foi possível entrar", detail: GenericFailure);

    private async Task<CurrentUserResponse> ToResponseAsync(ApplicationUser user)
    {
        var roles = (await users.GetRolesAsync(user)).ToList();
        var isStaff = roles.Contains(Roles.Admin) || roles.Contains(Roles.Professional);

        return new(user.Id, user.Email!, roles, user.ProfessionalId, user.PatientId,
            user.TwoFactorEnabled, MustSetupTwoFactor: isStaff && !user.TwoFactorEnabled);
    }

    private static string NormalizeTotp(string code) => code.Replace(" ", "").Replace("-", "");

    private static string FormatKey(string key) =>
        string.Join(' ', key.ToLowerInvariant().Chunk(4).Select(chunk => new string(chunk)));
}
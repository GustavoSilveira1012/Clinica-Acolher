using ClinicaAcolher.Application.Common;
using ClinicaAcolher.Application.PatientAccess;
using ClinicaAcolher.Domain.Common;
using ClinicaAcolher.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ClinicaAcolher.Api.Auth;

public sealed class IdentityPatientAccounts(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn)
    : IPatientAccounts
{
    public Task<bool> HasAccountAsync(Guid patientId, CancellationToken ct) =>
        users.Users.AnyAsync(u => u.PatientId == patientId, ct);

    public async Task<Guid> CreateAsync(Guid patientId, string email, string password, CancellationToken ct)
    {
        var normalized = email.Trim().ToLowerInvariant();
        var user = new ApplicationUser { UserName = normalized, Email = normalized, PatientId = patientId };

        var result = await users.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            if (result.Errors.Any(e => e.Code is "DuplicateUserName" or "DuplicateEmail"))
                throw new ConflictException("RN-POR-01", "Já existe uma conta com este e-mail. Fale com o seu profissional.");

            throw new DomainException("RN-SEG-02", "A senha não atende aos requisitos de segurança: use pelo menos 10 caracteres.");
        }

        await users.AddToRoleAsync(user, Roles.Patient);
        return user.Id;
    }

    public async Task SignInAsync(Guid userId)
    {
        var user = await users.FindByIdAsync(userId.ToString());
        if (user is not null)
            await signIn.SignInAsync(user, isPersistent: false);
    }
}
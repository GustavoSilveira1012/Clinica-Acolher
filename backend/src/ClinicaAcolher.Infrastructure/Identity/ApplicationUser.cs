using Microsoft.AspNetCore.Identity;

namespace ClinicaAcolher.Infrastructure.Identity;

public sealed class ApplicationUser : IdentityUser<Guid>
{
    public ApplicationUser() => Id = Guid.CreateVersion7();

    public Guid? ProfessionalId { get; set; }
    public Guid? PatientId { get; set; }
}
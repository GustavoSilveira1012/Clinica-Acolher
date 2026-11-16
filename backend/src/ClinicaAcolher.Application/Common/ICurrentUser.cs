namespace ClinicaAcolher.Application.Common;

public interface ICurrentUser
{
    bool IsClinicAdmin {get;}
    Guid? ProfessionalId {get;}
    Guid? PatientId {get;}
}
using ClinicaAcolher.Domain.Appointments;
using ClinicaAcolher.Domain.Common;
using ClinicaAcolher.Domain.Patients;

namespace ClinicaAcolher.Domain.Attendance;

public sealed class AttendancePolicyAcceptance : Entity
{
    public Guid PatientId { get; private set; }
    public Guid ProfessionalId { get; private set; }
    public Guid PolicyId { get; private set; }
    public int PolicyVersion { get; private set; }
    public DateTimeOffset AcceptedAt { get; private set; }

    private AttendancePolicyAcceptance() { }

    public static AttendancePolicyAcceptance Record(Patient patient, AttendancePolicy policy, DateTimeOffset now) =>
        new()
        {
            PatientId = patient.Id,
            ProfessionalId = policy.ProfessionalId,
            PolicyId = policy.Id,
            PolicyVersion = policy.Version,
            AcceptedAt = now,
        };

    // RN-CAN-01: vale a versão aceita até a data da sessão
    // RN-CAN-07: sem aceite, o resultado é nulo, e nada é cobrado
    public static AttendancePolicyAcceptance? ApplicableTo(
        IEnumerable<AttendancePolicyAcceptance> acceptances, Appointment appointment) =>
        acceptances
            .Where(a => a.PatientId == appointment.PatientId
                     && a.ProfessionalId == appointment.ProfessionalId
                     && a.AcceptedAt <= appointment.StartsAt)
            .OrderByDescending(a => a.AcceptedAt)
            .FirstOrDefault();
}
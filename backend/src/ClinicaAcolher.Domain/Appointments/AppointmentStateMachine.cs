namespace ClinicaAcolher.Domain.Appointments;

public static class AppointmentStateMachine
{
    private static readonly Dictionary<AppointmentStatus, AppointmentStatus[]> Allowed = new()
    {
        [AppointmentStatus.Scheduled] =
        [
            AppointmentStatus.Confirmed, AppointmentStatus.Completed, AppointmentStatus.NoShow,
            AppointmentStatus.Cancelled, AppointmentStatus.Rescheduled,
        ],
        [AppointmentStatus.Confirmed] =
        [
            AppointmentStatus.Completed, AppointmentStatus.NoShow,
            AppointmentStatus.Cancelled, AppointmentStatus.Rescheduled,
        ],
        // RN-AGD-08: estados finais não saem para lugar nenhum
        [AppointmentStatus.Completed] = [],
        [AppointmentStatus.NoShow] = [],
        [AppointmentStatus.Cancelled] = [],
        [AppointmentStatus.Rescheduled] = [],
    };

    public static bool CanTransition(AppointmentStatus from, AppointmentStatus to) =>
        Allowed[from].Contains(to);

    public static bool IsFinal(AppointmentStatus status) => Allowed[status].Length == 0;
}
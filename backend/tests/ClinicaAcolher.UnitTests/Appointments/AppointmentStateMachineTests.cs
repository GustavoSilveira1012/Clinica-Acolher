using ClinicaAcolher.Domain.Appointments;

namespace ClinicaAcolher.UnitTests.Appointments;

public class AppointmentStateMachineTests
{
    [Theory]
    [InlineData(AppointmentStatus.Scheduled, AppointmentStatus.Confirmed)]
    [InlineData(AppointmentStatus.Scheduled, AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.Confirmed, AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Confirmed, AppointmentStatus.NoShow)]
    [InlineData(AppointmentStatus.Confirmed, AppointmentStatus.Rescheduled)]
    public void Permite_transicoes_validas(AppointmentStatus de, AppointmentStatus para)
    {
        Assert.True(AppointmentStateMachine.CanTransition(de, para));
    }

    [Theory]
    [InlineData(AppointmentStatus.Confirmed, AppointmentStatus.Scheduled)]
    [InlineData(AppointmentStatus.Completed, AppointmentStatus.Cancelled)]
    [InlineData(AppointmentStatus.Cancelled, AppointmentStatus.Confirmed)]
    [InlineData(AppointmentStatus.NoShow, AppointmentStatus.Completed)]
    [InlineData(AppointmentStatus.Rescheduled, AppointmentStatus.Scheduled)]
    public void RN_AGD_08_bloqueia_transicoes_invalidas(AppointmentStatus de, AppointmentStatus para)
    {
        Assert.False(AppointmentStateMachine.CanTransition(de, para));
    }

    [Fact]
    public void Todo_status_tem_regra_definida()
    {
        foreach (var status in Enum.GetValues<AppointmentStatus>())
            _ = AppointmentStateMachine.IsFinal(status);
    }
}
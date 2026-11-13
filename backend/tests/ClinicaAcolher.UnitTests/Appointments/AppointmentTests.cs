using ClinicaAcolher.Domain.Appointments;
using ClinicaAcolher.Domain.Common;
using ClinicaAcolher.Domain.Patients;
using ClinicaAcolher.Domain.Professionals;
using ClinicaAcolher.Domain.Relationships;

namespace ClinicaAcolher.UnitTests.Appointments;

public class AppointmentTests
{
    private static readonly DateTimeOffset Agora = new(2026, 11, 12, 15, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset Inicio = Agora.AddDays(7);
    private static readonly TimeSpan Cinquenta = TimeSpan.FromMinutes(50);

    private static TherapeuticRelationship VinculoAtivo()
    {
        var paciente = Patient.Create("Paciente Teste", new DateOnly(1990, 5, 20), Agora);
        var profissional = Professional.Create("Profissional Teste", "Teste", "prof@exemplo.org",
            ProfessionalCategory.Psychoanalyst, null, Agora);
        profissional.Approve(Agora);
        return TherapeuticRelationship.Start(paciente, profissional, Agora);
    }

    private static Appointment SessaoAgendada() =>
        Appointment.Schedule(VinculoAtivo(), Inicio, Cinquenta, Agora);

    [Fact]
    public void RN_VIN_04_nao_agenda_com_vinculo_suspenso()
    {
        var vinculo = VinculoAtivo();
        vinculo.Suspend();

        var erro = Assert.Throws<DomainException>(() => Appointment.Schedule(vinculo, Inicio, Cinquenta, Agora));

        Assert.Equal("RN-VIN-04", erro.Rule);
    }

    [Theory]
    [InlineData(10)]
    [InlineData(200)]
    public void RN_AGD_01_rejeita_duracao_fora_do_limite(int minutos)
    {
        var erro = Assert.Throws<DomainException>(() =>
            Appointment.Schedule(VinculoAtivo(), Inicio, TimeSpan.FromMinutes(minutos), Agora));

        Assert.Equal("RN-AGD-01", erro.Rule);
    }

    [Fact]
    public void RN_AGD_09_guarda_o_horario_em_utc()
    {
        var quinzeHorasEmBrasilia = new DateTimeOffset(2026, 11, 20, 15, 0, 0, TimeSpan.FromHours(-3));

        var sessao = Appointment.Schedule(VinculoAtivo(), quinzeHorasEmBrasilia, Cinquenta, Agora);

        Assert.Equal(TimeSpan.Zero, sessao.StartsAt.Offset);
        Assert.Equal(18, sessao.StartsAt.Hour);
        Assert.Equal(Cinquenta, sessao.Duration);
    }

    [Fact]
    public void RN_AGD_11_nao_registra_sessao_realizada_antes_do_horario()
    {
        var sessao = SessaoAgendada();

        var erro = Assert.Throws<DomainException>(() => sessao.Complete(Inicio.AddMinutes(-1)));

        Assert.Equal("RN-AGD-11", erro.Rule);
        Assert.Equal(AppointmentStatus.Scheduled, sessao.Status);
    }

    [Fact]
    public void RN_AGD_11_registra_falta_depois_do_horario()
    {
        var sessao = SessaoAgendada();

        sessao.MarkNoShow(Inicio.AddMinutes(20));

        Assert.Equal(AppointmentStatus.NoShow, sessao.Status);
    }

    [Fact]
    public void RN_AGD_08_sessao_cancelada_nao_vira_realizada()
    {
        var sessao = SessaoAgendada();
        sessao.Cancel(CancellationSource.Patient, "Imprevisto", Agora);

        var erro = Assert.Throws<DomainException>(() => sessao.Complete(Inicio.AddHours(1)));

        Assert.Equal("RN-AGD-08", erro.Rule);
    }

    [Fact]
    public void RN_CAN_06_cancelamento_registra_quem_motivo_e_antecedencia()
    {
        var sessao = SessaoAgendada();

        sessao.Cancel(CancellationSource.Patient, "  Imprevisto no trabalho  ", Inicio.AddHours(-30));

        Assert.Equal(CancellationSource.Patient, sessao.CancelledBy);
        Assert.Equal("Imprevisto no trabalho", sessao.CancellationReason);
        Assert.Equal(TimeSpan.FromHours(30), sessao.CancellationNotice);
    }

    [Fact]
    public void RN_CAN_06_cancelamento_exige_motivo()
    {
        var sessao = SessaoAgendada();

        var erro = Assert.Throws<DomainException>(() => sessao.Cancel(CancellationSource.Patient, " ", Agora));

        Assert.Equal("RN-CAN-06", erro.Rule);
        Assert.Equal(AppointmentStatus.Scheduled, sessao.Status);
    }

    [Fact]
    public void RN_CAN_03_remarcacao_aponta_para_a_nova_sessao()
    {
        var antiga = SessaoAgendada();
        var nova = Appointment.Schedule(VinculoAtivo(), Inicio.AddDays(1), Cinquenta, Agora);

        antiga.MarkRescheduled(nova.Id, Agora);

        Assert.Equal(AppointmentStatus.Rescheduled, antiga.Status);
        Assert.Equal(nova.Id, antiga.RescheduledToId);
        Assert.Equal(Inicio, antiga.StartsAt);
    }
}
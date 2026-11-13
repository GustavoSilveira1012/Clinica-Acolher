using ClinicaAcolher.Domain.Appointments;
using ClinicaAcolher.Domain.Attendance;
using ClinicaAcolher.Domain.Common;
using ClinicaAcolher.Domain.Professionals;
using static ClinicaAcolher.UnitTests.Cenarios;

namespace ClinicaAcolher.UnitTests.Attendance;

public class AttendancePolicyTests
{
    private readonly Professional _profissional = ProfissionalAprovado();

    // Política padrão: 24 horas de antecedência, cancelamento tardio 100%, falta 100%
    private AttendancePolicy Politica(int tardio = 100, int falta = 100) =>
        AttendancePolicy.Publish(_profissional, 1, 24, tardio, falta, Agora);

    private Appointment Sessao() => Cenarios.Sessao(Paciente(), _profissional);

    [Fact]
    public void RN_CAN_01_publica_politica_valida()
    {
        var politica = Politica();

        Assert.Equal(1, politica.Version);
        Assert.Equal(TimeSpan.FromHours(24), politica.MinimumNotice);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public void RN_CAN_01_rejeita_percentual_fora_de_0_a_100(int percentual)
    {
        var erro = Assert.Throws<DomainException>(() => Politica(tardio: percentual));

        Assert.Equal("RN-CAN-01", erro.Rule);
    }

    [Fact]
    public void Cancelamento_com_antecedencia_nao_cobra()
    {
        var sessao = Sessao();
        sessao.Cancel(CancellationSource.Patient, "Imprevisto", Inicio.AddHours(-30));

        Assert.Equal(ChargeDecision.NoCharge, Politica().Evaluate(sessao));
    }

    [Fact]
    public void Cancelamento_tardio_do_paciente_cobra()
    {
        var sessao = Sessao();
        sessao.Cancel(CancellationSource.Patient, "Imprevisto", Inicio.AddHours(-2));

        var decisao = Politica().Evaluate(sessao);

        Assert.Equal(new ChargeDecision(100, ChargeReason.LateCancellation), decisao);
        Assert.True(decisao.IsChargeable);
    }

    [Theory]
    [InlineData(CancellationSource.Professional)]
    [InlineData(CancellationSource.Platform)]
    public void Cancelamento_tardio_que_nao_e_do_paciente_nunca_cobra(CancellationSource quem)
    {
        var sessao = Sessao();
        sessao.Cancel(quem, "Imprevisto", Inicio.AddHours(-1));

        Assert.Equal(ChargeDecision.NoCharge, Politica().Evaluate(sessao));
    }

    [Fact]
    public void Falta_cobra_o_percentual_de_falta()
    {
        var sessao = Sessao();
        sessao.MarkNoShow(Inicio.AddMinutes(20));

        Assert.Equal(new ChargeDecision(50, ChargeReason.NoShow), Politica(falta: 50).Evaluate(sessao));
    }

    [Fact]
    public void Sessao_realizada_nao_cobra()
    {
        var sessao = Sessao();
        sessao.Complete(Inicio.AddMinutes(50));

        Assert.Equal(ChargeDecision.NoCharge, Politica().Evaluate(sessao));
    }

    [Fact]
    public void Politica_com_percentual_zero_nao_cobra()
    {
        var sessao = Sessao();
        sessao.Cancel(CancellationSource.Patient, "Imprevisto", Inicio.AddHours(-1));

        Assert.Equal(ChargeDecision.NoCharge, Politica(tardio: 0).Evaluate(sessao));
    }

    [Fact]
    public void RN_CAN_07_sem_aceite_nenhum_aceite_se_aplica()
    {
        var aplicavel = AttendancePolicyAcceptance.ApplicableTo([], Sessao());

        Assert.Null(aplicavel);
    }

    [Fact]
    public void RN_CAN_01_vale_a_versao_aceita_ate_a_data_da_sessao()
    {
        var paciente = Paciente();
        var sessao = Cenarios.Sessao(paciente, _profissional);
        var v1 = AttendancePolicy.Publish(_profissional, 1, 24, 100, 100, Agora);
        var v2 = AttendancePolicy.Publish(_profissional, 2, 48, 100, 100, Agora);

        var aceites = new[]
        {
            AttendancePolicyAcceptance.Record(paciente, v1, Agora),
            AttendancePolicyAcceptance.Record(paciente, v2, Inicio.AddDays(1)),
        };

        var aplicavel = AttendancePolicyAcceptance.ApplicableTo(aceites, sessao);

        Assert.Equal(1, aplicavel!.PolicyVersion);
    }
}
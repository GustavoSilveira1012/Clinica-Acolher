using ClinicaAcolher.Application.Appointments;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaAcolher.Api.Controllers;

[ApiController]
[Route("api/v1/appointments")]
public sealed class AppointmentsController(AppointmentService service) : ControllerBase
{
    private static readonly TimeSpan MaxAgendaPeriod = TimeSpan.FromDays(31);

    [HttpPost]
    public async Task<ActionResult<AppointmentResponse>> Schedule(ScheduleAppointmentRequest request, CancellationToken ct)
    {
        var appointment = await service.ScheduleAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { id = appointment.Id }, appointment);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AppointmentResponse>> GetById(Guid id, CancellationToken ct) =>
        Ok(await service.GetByIdAsync(id, ct));

    [HttpGet("/api/v1/professionals/{professionalId:guid}/agenda")]
    public async Task<ActionResult<IReadOnlyList<AgendaItemResponse>>> Agenda(
        Guid professionalId, [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to, CancellationToken ct)
    {
        if (to <= from || to - from > MaxAgendaPeriod)
            return ValidationProblem(detail: "O período precisa terminar depois de começar e ter no máximo 31 dias.");

        return Ok(await service.GetAgendaAsync(professionalId, from, to, ct));
    }
}
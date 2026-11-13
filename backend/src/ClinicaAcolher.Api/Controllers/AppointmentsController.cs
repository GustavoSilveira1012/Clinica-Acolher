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

    [HttpPost("{id:guid}/confirm")]
    public async Task<ActionResult<AppointmentResponse>> Confirm(Guid id, CancellationToken ct) =>
        Ok(await service.ConfirmAsync(id, ct));

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<AppointmentResponse>> Complete(Guid id, CancellationToken ct) =>
        Ok(await service.CompleteAsync(id, ct));

    [HttpPost("{id:guid}/no-show")]
    public async Task<ActionResult<AppointmentResponse>> NoShow(Guid id, CancellationToken ct) =>
        Ok(await service.MarkNoShowAsync(id, ct));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<AppointmentResponse>> Cancel(Guid id, CancelAppointmentRequest request, CancellationToken ct) =>
        Ok(await service.CancelAsync(id, request, ct));

    [HttpPost("{id:guid}/reschedule")]
    public async Task<ActionResult<AppointmentResponse>> Reschedule(Guid id, RescheduleAppointmentRequest request, CancellationToken ct)
    {
        var replacement = await service.RescheduleAsync(id, request, ct);
        return CreatedAtAction(nameof(GetById), new { id = replacement.Id }, replacement);
    }

    [HttpGet("/api/v1/professionals/{professionalId:guid}/agenda")]
    public async Task<ActionResult<IReadOnlyList<AgendaItemResponse>>> Agenda(
        Guid professionalId, [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to, CancellationToken ct)
    {
        if (to <= from || to - from > MaxAgendaPeriod)
            return ValidationProblem(detail: "O período precisa terminar depois de começar e ter no máximo 31 dias.");

        return Ok(await service.GetAgendaAsync(professionalId, from, to, ct));
    }
}

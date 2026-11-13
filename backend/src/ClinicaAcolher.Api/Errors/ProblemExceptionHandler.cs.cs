using ClinicaAcolher.Application.Common;
using ClinicaAcolher.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaAcolher.Api.Errors;

public sealed class ProblemExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        (int Status, string Title, string? Code)? problem = exception switch
        {
            DomainException e => (StatusCodes.Status422UnprocessableEntity, "Regra de negócio violada", e.Rule),
            ConflictException e => (StatusCodes.Status409Conflict, "Conflito com dados existentes", e.Rule),
            NotFoundException => (StatusCodes.Status404NotFound, "Recurso não encontrado", null),
            _ => null,
        };

        if (problem is not { } p)
            return false;

        context.Response.StatusCode = p.Status;

        var details = new ProblemDetails { Status = p.Status, Title = p.Title, Detail = exception.Message };
        if (p.Code is not null)
            details.Extensions["code"] = p.Code;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = details,
        });
    }
}
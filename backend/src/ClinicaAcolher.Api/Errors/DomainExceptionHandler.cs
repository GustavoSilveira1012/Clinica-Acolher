using ClinicaAcolher.Domain.Common;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ClinicaAcolher.Api.Errors;

public sealed class DomainExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext context, Exception exception, CancellationToken ct)
    {
        if (exception is not DomainException domainError)
            return false;

        context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;

        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Regra de negócio violada",
                Detail = domainError.Message,
                Extensions = { ["code"] = domainError.Rule },
            },
        });
    }
}
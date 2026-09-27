using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Uinsure.Domain.Policies;

namespace Uinsure.Api.Errors;

internal sealed class ApiExceptionHandler(IProblemDetailsService problemDetailsService)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is DomainConflictException conflict)
        {
            httpContext.Response.StatusCode = StatusCodes.Status409Conflict;
            var conflictProblem = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "The requested change conflicts with current policy state.",
                Detail = conflict.Message,
                Type = "https://httpstatuses.com/409"
            };
            ApiProblemDetailsDefaults.AddExtensions(conflictProblem, httpContext, "policy_conflict");
            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = conflictProblem,
                Exception = exception
            });
        }

        if (exception is not DomainValidationException validation)
        {
            return false;
        }

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        var problem = new HttpValidationProblemDetails(validation.Errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "One or more validation errors occurred.",
            Type = "https://httpstatuses.com/400"
        };
        ApiProblemDetailsDefaults.AddExtensions(problem, httpContext, "validation_error");

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }
}

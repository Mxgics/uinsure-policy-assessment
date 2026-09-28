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
        ProblemDetails problem;
        string code;
        switch (exception)
        {
            case DomainConflictException conflict:
                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "The requested change conflicts with current policy state.",
                    Detail = conflict.Message,
                    Type = "https://httpstatuses.com/409"
                };
                code = "policy_conflict";
                break;
            case DomainValidationException validation:
                problem = new HttpValidationProblemDetails(validation.Errors)
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "One or more validation errors occurred.",
                    Type = "https://httpstatuses.com/400"
                };
                code = "validation_error";
                break;
            default:
                return false;
        }

        httpContext.Response.StatusCode = problem.Status.Value;
        ApiProblemDetailsDefaults.AddExtensions(problem, httpContext, code);

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
            Exception = exception
        });
    }
}

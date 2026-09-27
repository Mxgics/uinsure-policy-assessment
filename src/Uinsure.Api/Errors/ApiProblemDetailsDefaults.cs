using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Uinsure.Api.Errors;

internal static class ApiProblemDetailsDefaults
{
    public static void AddExtensions(
        ProblemDetails problem,
        HttpContext httpContext,
        string code)
    {
        problem.Extensions.TryAdd("code", code);
        problem.Extensions.TryAdd(
            "traceId",
            Activity.Current?.Id ?? httpContext.TraceIdentifier);
    }

    public static string CodeFor(int? status) => status switch
    {
        StatusCodes.Status400BadRequest => "bad_request",
        StatusCodes.Status404NotFound => "not_found",
        StatusCodes.Status409Conflict => "conflict",
        _ => "internal_error"
    };
}

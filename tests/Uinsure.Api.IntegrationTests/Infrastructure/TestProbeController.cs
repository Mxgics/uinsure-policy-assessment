using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Uinsure.Api.IntegrationTests.Infrastructure;

[ApiController]
[Route("__tests")]
public sealed class TestProbeController : ControllerBase
{
    [HttpPost("validation")]
    public IActionResult Validate(ProbeRequest request) => Ok(request);

    [HttpGet("exception")]
    public IActionResult Throw() => throw new InvalidOperationException("Sensitive test exception text.");
}

public sealed record ProbeRequest(
    [Required] string? Name,
    [Range(1, 3)] int Count);

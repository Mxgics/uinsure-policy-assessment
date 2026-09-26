using Microsoft.AspNetCore.Mvc;
using Uinsure.Api.Policies;

namespace Uinsure.Api.Controllers;

[ApiController]
[Route("api/policies")]
public sealed class PoliciesController(PolicyService policyService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<PolicyResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PolicyResponse>> Sell(
        SellPolicyRequest request,
        CancellationToken cancellationToken)
    {
        var policy = await policyService.SellAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { reference = policy.Reference }, policy);
    }

    [HttpGet("{reference}")]
    [ProducesResponseType<PolicyResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PolicyResponse>> Get(
        string reference,
        CancellationToken cancellationToken)
    {
        var policy = await policyService.GetAsync(reference, cancellationToken);
        return policy is null ? NotFound() : Ok(policy);
    }

    [HttpGet("{reference}/terms/{termId:guid}")]
    [ProducesResponseType<PolicyTermResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PolicyTermResponse>> GetTerm(
        string reference,
        Guid termId,
        CancellationToken cancellationToken)
    {
        var term = await policyService.GetTermAsync(reference, termId, cancellationToken);
        return term is null ? NotFound() : Ok(term);
    }
}

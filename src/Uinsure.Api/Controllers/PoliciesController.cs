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

    [HttpGet("{reference}/terms/{termId:guid}/cancellation-quote")]
    [ProducesResponseType<CancellationResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CancellationResponse>> QuoteCancellation(
        string reference,
        Guid termId,
        [FromQuery] DateOnly? date,
        CancellationToken cancellationToken)
    {
        if (date is null)
        {
            ModelState.AddModelError("date", "Date is required.");
            return ValidationProblem(ModelState);
        }
        var quote = await policyService.QuoteCancellationAsync(reference, termId, date.Value, cancellationToken);
        return quote is null ? NotFound() : Ok(quote);
    }

    [HttpPost("{reference}/terms/{termId:guid}/cancellations")]
    [ProducesResponseType<CancellationResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CancellationResponse>> Cancel(
        string reference,
        Guid termId,
        CancellationToken cancellationToken)
    {
        var cancellation = await policyService.CancelAsync(reference, termId, cancellationToken);
        return cancellation is null
            ? NotFound()
            : CreatedAtAction(nameof(GetTerm), new { reference, termId }, cancellation);
    }

    [HttpPost("{reference}/terms/{termId:guid}/renewals")]
    [ProducesResponseType<PolicyTermResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PolicyTermResponse>> Renew(
        string reference,
        Guid termId,
        RenewPolicyRequest request,
        CancellationToken cancellationToken)
    {
        var successor = await policyService.RenewAsync(reference, termId, request, cancellationToken);
        return successor is null
            ? NotFound()
            : CreatedAtAction(
                nameof(GetTerm),
                new { reference, termId = successor.Id },
                successor);
    }
}

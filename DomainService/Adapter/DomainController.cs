using DomainService.Application.Commands;
using DomainService.Application.Dto;
using DomainService.Application.Dto.Request;
using DomainService.Application.Dto.Response;
using DomainService.Application.Queries;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace DomainService.Adapter;

[ApiController]
[Route("api/[controller]")]
public class DomainsController : ControllerBase
{
    private readonly IMessageBus _messageBus;

    public DomainsController(IMessageBus messageBus)
    {
        _messageBus = messageBus;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<DomainDto>> GetDomain([FromRoute] Guid id)
    {
        var query = new GetDomainByIdQuery(id);
        var result = await _messageBus.InvokeAsync<DomainDto?>(query);

        return result == null ? NotFound() : Ok(result);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<PagedResult<DomainSummaryDto>>> GetUserDomains(
        [FromRoute] string userId,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50)
    {
        var query = new GetDomainsByUserQuery(userId, skip, take);
        var result = await _messageBus.InvokeAsync<PagedResult<DomainSummaryDto>>(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<DomainDto>> CreateDomain([FromBody] CreateDomainCommand request)
    {
        try
        {
            var command = new CreateDomainCommand(request.DomainName, request.UserId);
            var result = await _messageBus.InvokeAsync<DomainDto>(command);
            return CreatedAtAction(nameof(GetDomain), new { id = result.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}/name")]
    public async Task<IActionResult> UpdateDomainName([FromRoute] Guid id, [FromBody] UpdateDomainNameRequest request)
    {
        try
        {
            var command = new UpdateDomainNameCommand(id, request.NewDomainName, request.UserId);
            await _messageBus.InvokeAsync(command);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteDomain([FromRoute] Guid id, [FromQuery] string userId)
    {
        try
        {
            var command = new DeleteDomainCommand(id, userId);
            await _messageBus.InvokeAsync(command);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/verify")]
    public async Task<ActionResult<bool>> VerifyDomain([FromRoute] Guid id, [FromBody] VerifyDomainRequest request)
    {
        var command = new VerifyDomainCommand(id, request.VerificationToken, request.UserId);
        var result = await _messageBus.InvokeAsync<bool>(command);
        return Ok(new { verified = result });
    }

    [HttpPost("{id}/restore")]
    public async Task<IActionResult> RestoreDomain(Guid id, [FromBody] RestoreDomainCommand request)
    {
        try
        {
            await _messageBus.InvokeAsync(request);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
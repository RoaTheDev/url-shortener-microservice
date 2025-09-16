namespace DomainService.Application.Dto.Response;
public record DomainSummaryDto
{
    public Guid Id { get; init; }
    public string DomainName { get; init; } = string.Empty;
    public bool IsVerified { get; init; }
    public DateTime CreatedAt { get; init; }
}

namespace DomainService.Application.Dto.Response;

public record DomainDto
{
    public Guid Id { get; init; }
    public string DomainName { get; init; } = string.Empty;
    public string UserId { get; init; } = string.Empty;
    public bool IsVerified { get; init; }
    public bool IsDeleted { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
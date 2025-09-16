namespace DomainService.Application.Dto.Response;

public record CreateDomainResult(Guid DomainId);
public record VerifyDomainResult(bool IsVerified);
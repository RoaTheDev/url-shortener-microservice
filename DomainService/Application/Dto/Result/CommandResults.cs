namespace DomainService.Application.Dto.Result;

public record CreateDomainResult(Guid DomainId);
public record VerifyDomainResult(bool IsVerified);
namespace SharedContracts.Domain;

public record DomainVerificationEvent(
    Guid DomainId,
    string DomainName,
    string UserId) : IDomainEvent;
namespace SharedContracts.Domain;
public record DomainCreatedEvent(Guid DomainId, string DomainName, string UserId) : IDomainEvent;

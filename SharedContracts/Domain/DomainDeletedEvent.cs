namespace SharedContracts.Domain;

public record DomainDeletedEvent(Guid Id, string UserId) : IDomainEvent;

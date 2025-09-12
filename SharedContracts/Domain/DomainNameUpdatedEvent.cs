namespace SharedContracts.Domain;

public record DomainNameUpdatedEvent(Guid Id, string DomainName,string UserId)  : IDomainEvent;
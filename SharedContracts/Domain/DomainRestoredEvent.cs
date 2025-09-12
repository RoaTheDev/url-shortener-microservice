namespace SharedContracts.Domain;

public class DomainRestoredEvent(Guid Id, string UserId) : IDomainEvent;
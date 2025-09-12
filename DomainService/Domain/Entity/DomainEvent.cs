using SharedContracts;

namespace DomainService.Domain.Entity;

public class DomainEvent
{
    protected readonly List<IDomainEvent> _domainEvents = [];
    public IReadOnlyCollection<IDomainEvent> Events => _domainEvents.AsReadOnly();
    public void ClearEvents() => _domainEvents.Clear();
}
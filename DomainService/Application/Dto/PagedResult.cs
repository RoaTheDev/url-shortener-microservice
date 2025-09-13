namespace DomainService.Application.Dto;

public record PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = [];
    public int Total { get; init; }
    public int Skip { get; init; }
    public int Take { get; init; }
    public bool HasMore => Skip + Take < Total;
}
namespace SharedContracts;

public record UrlCreatedEvent(
    Guid UrlId,
    string ShortCode,
    string OriginalUrl,
    Guid UserId,
    DateTime CreatedAt
);

public record UrlAccessedEvent(
    Guid UrlId,
    string ShortCode,
    string UserAgent,
    string IpAddress,
    DateTime AccessedAt);

public record UrlDeletedEvent(
    Guid UrlId,
    string ShortCode,
    Guid UserId,
    DateTime DeletedAt);
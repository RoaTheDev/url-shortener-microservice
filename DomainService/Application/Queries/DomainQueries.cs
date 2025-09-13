namespace DomainService.Application.Queries;

public record GetDomainByIdQuery(Guid DomainId);
public record GetDomainByIdAndUserQuery(Guid DomainId, string UserId);
public record GetDomainsByUserQuery(string UserId, int Skip = 0, int Take = 50);
public record GetAllDomainsQuery(int Skip = 0, int Take = 50);
public record GetVerifiedDomainsQuery(string UserId);
public record GetDeletedDomainsQuery(string UserId);
public record CheckDomainExistsQuery(string DomainName, string UserId);

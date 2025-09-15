using DomainService.Application.Commands;
using DomainService.Infra.Persistent;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;
using Wolverine.EntityFrameworkCore;

namespace DomainService.Application.Handlers;

public class RestoreDomainHandler
{
    [Transactional]
    public static async Task Handle(
        RestoreDomainCommand command,
        IDbContextOutbox<AppDbContext> context)
    {
        var domain = await context.DbContext.Domains
            .FirstOrDefaultAsync(d => d.Id == command.DomainId && d.UserId == command.UserId);

        if (domain == null)
            throw new InvalidOperationException("Domain not found or access denied");

        if (!domain.IsDeleted)
            throw new InvalidOperationException("Domain is not deleted");

        var existingDomain = await context.DbContext.Domains
            .FirstOrDefaultAsync(d => d.DomainName == domain.DomainName
                                      && d.UserId == command.UserId
                                      && !d.IsDeleted);

        if (existingDomain != null)
            throw new InvalidOperationException($"Cannot restore: Domain '{domain.DomainName}' already exists");

        domain.Restore();
        await context.PublishAsync(domain.Events);

        domain.ClearEvents();
        await context.SaveChangesAndFlushMessagesAsync();
    }

}
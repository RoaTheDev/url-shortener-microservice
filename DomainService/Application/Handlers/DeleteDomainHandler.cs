using DomainService.Application.Commands;
using DomainService.Infra.Persistent;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;
using Wolverine.EntityFrameworkCore;

namespace DomainService.Application.Handlers;

public class DeleteDomainHandler
{
    [Transactional]
    public static async Task Handle(
        DeleteDomainCommand command,
        IDbContextOutbox<AppDbContext> context)
    {
        var domain = await context.DbContext.Domains
            .FirstOrDefaultAsync(d => d.Id == command.DomainId && d.UserId == command.UserId);

        if (domain == null)
            throw new InvalidOperationException("Domain not found or access denied");

        if (domain.IsDeleted)
            throw new InvalidOperationException("Domain is already deleted");

        domain.Delete();
        await context.PublishAsync(domain.Events);

        domain.ClearEvents();
        await context.SaveChangesAndFlushMessagesAsync();
    }

}
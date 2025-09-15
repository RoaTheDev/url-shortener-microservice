using DomainService.Application.Commands;
using DomainService.Infra.Persistent;
using Microsoft.EntityFrameworkCore;
using Wolverine.Attributes;
using Wolverine.EntityFrameworkCore;

namespace DomainService.Application.Handlers;

public class UpdateDomainNameHandler 
{
    [Transactional]
    public static async Task Handle(
        UpdateDomainNameCommand command,
        IDbContextOutbox<AppDbContext> context,
        ILogger<VerifyDomainHandler> logger)
    {
        var domain = await context.DbContext.Domains
            .FirstOrDefaultAsync(d => d.Id == command.DomainId
                                      && d.UserId == command.UserId
                                      && !d.IsDeleted);

        if (domain == null)
            throw new InvalidOperationException("Domain not found or access denied");

        if (domain.DomainName == command.NewDomainName)
        {
            throw new InvalidOperationException($"Domain '{command.NewDomainName}' already exists for this user");
        }

        domain.UpdateDomainName(command.NewDomainName);

        logger.LogInformation($"Publishing {domain.Events.Count} events for domain {domain.Id}");
        foreach (var evt in domain.Events)
        {
            logger.LogInformation($"Publishing event: {evt.GetType().Name}");
        }

        await context.PublishAsync(domain.Events);

        domain.ClearEvents();
        await context.SaveChangesAndFlushMessagesAsync();

        logger.LogInformation($"Successfully updated domain {domain.Id} and published events");
    }

}
using DomainService.Application.Commands;
using DomainService.Application.Dto.Response;
using DomainService.Infra.Persistent;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.EntityFrameworkCore;

namespace DomainService.Application.Handlers;

public class CreateDomainHandler : IWolverineHandler
{
    [Transactional]
    public async Task<CreateDomainResult> Handle(
        CreateDomainCommand command,
        IDbContextOutbox<AppDbContext> context,
        ILogger<CreateDomainHandler> logger)
    {
        var existingDomain = await context.DbContext.Domains
            .FirstOrDefaultAsync(d => d.DomainName == command.DomainName
                                      && d.UserId == command.UserId
                                      && !d.IsDeleted);

        if (existingDomain != null)
            throw new InvalidOperationException($"Domain '{command.DomainName}' already exists for this user");

        var domain = Domain.Entity.Domain.Create(command.DomainName, command.UserId);

        context.DbContext.Add(domain);
        await context.PublishAsync(domain.Events);

        logger.LogInformation(@"Before calling {}", string.Join(", ", domain.Events));
        domain.ClearEvents();

        await context.SaveChangesAndFlushMessagesAsync();
        logger.LogInformation("After calling {}", string.Join(", ", domain.Events));

        return new CreateDomainResult(domain.Id);
    }

}
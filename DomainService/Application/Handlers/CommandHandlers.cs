using DomainService.Application.Commands;
using DomainService.Application.Dto;
using DomainService.Infra.Persistent;
using ImTools;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.EntityFrameworkCore;

namespace DomainService.Application.Handlers;

public class CommandHandlers : IWolverineHandler
{
    private readonly ILogger<CommandHandlers> _logger;

    public CommandHandlers(ILogger<CommandHandlers> logger)
    {
        _logger = logger;
    }

    [Transactional]
    public async Task<DomainDto> Handle(
        CreateDomainCommand command,
        IDbContextOutbox<AppDbContext> context)
    {
        // Validation - check if domain already exists for this user
        var existingDomain = await context.DbContext.Domains
            .FirstOrDefaultAsync(d => d.DomainName == command.DomainName
                                      && d.UserId == command.UserId
                                      && !d.IsDeleted);

        if (existingDomain != null)
            throw new InvalidOperationException($"Domain '{command.DomainName}' already exists for this user");

        // Create domain entity
        var domain = Domain.Entity.Domain.Create(command.DomainName, command.UserId);

        context.DbContext.Add(domain);
        await context.PublishAsync(domain.Events);

        _logger.LogInformation(@"Before calling {}", string.Join(", ", domain.Events));
        domain.ClearEvents();

        await context.SaveChangesAndFlushMessagesAsync();
        _logger.LogInformation("After calling {}", string.Join(", ", domain.Events));

        // Return DTO
        return new DomainDto
        {
            Id = domain.Id,
            DomainName = domain.DomainName,
            UserId = domain.UserId,
            IsVerified = domain.IsVerified,
            IsDeleted = domain.IsDeleted,
            CreatedAt = domain.CreatedAt,
            UpdatedAt = domain.UpdatedAt
        };
    }

    [Transactional]
    public class DeleteDomainHandler : IWolverineHandler
    {
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

        // Check if domain name is available again
        var existingDomain = await context.DbContext.Domains
            .FirstOrDefaultAsync(d => d.DomainName == domain.DomainName
                                      && d.UserId == command.UserId
                                      && !d.IsDeleted);

        if (existingDomain != null)
            throw new InvalidOperationException($"Cannot restore: Domain '{domain.DomainName}' already exists");

        // Restore domain
        domain.Restore();
        await context.PublishAsync(domain.Events);

        // Publish events

        domain.ClearEvents();
        await context.SaveChangesAndFlushMessagesAsync();
    }

    [Transactional]
    public static async Task Handle(
        UpdateDomainNameCommand command,
        IDbContextOutbox<AppDbContext> context,
        ILogger<CommandHandlers> logger) // Add logger
    {
        var domain = await context.DbContext.Domains
            .FirstOrDefaultAsync(d => d.Id == command.DomainId
                                      && d.UserId == command.UserId
                                      && !d.IsDeleted);

        if (domain == null)
            throw new InvalidOperationException("Domain not found or access denied");

        // Check if new domain name already exists
        if (domain.DomainName == command.NewDomainName)
        {
            throw new InvalidOperationException($"Domain '{command.NewDomainName}' already exists for this user");
        }

        // Update domain
        domain.UpdateDomainName(command.NewDomainName);

        // Log events before publishing
        logger.LogInformation($"Publishing {domain.Events.Count} events for domain {domain.Id}");
        foreach (var evt in domain.Events)
        {
            logger.LogInformation($"Publishing event: {evt.GetType().Name}");
        }

        await context.PublishAsync(domain.Events);
        // Publish events

        domain.ClearEvents();
        await context.SaveChangesAndFlushMessagesAsync();

        logger.LogInformation($"Successfully updated domain {domain.Id} and published events");
    }
}
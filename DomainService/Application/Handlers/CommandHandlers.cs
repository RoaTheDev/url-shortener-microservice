using DomainService.Application.Commands;
using DomainService.Application.Dto;
using DomainService.Infra.Persistent;
using Microsoft.EntityFrameworkCore;
using Wolverine;

namespace DomainService.Application.Handlers;

public class CommandHandlers : IWolverineHandler
{
    public async Task<DomainDto> Handle(
        CreateDomainCommand command,
        AppDbContext context,
        IMessageContext messageContext)
    {
        // Validation - check if domain already exists for this user
        var existingDomain = await context.Domains
            .FirstOrDefaultAsync(d => d.DomainName == command.DomainName
                                      && d.UserId == command.UserId
                                      && !d.IsDeleted);

        if (existingDomain != null)
            throw new InvalidOperationException($"Domain '{command.DomainName}' already exists for this user");

        // Create domain entity
        var domain = Domain.Entity.Domain.Create(command.DomainName, command.UserId);

        // Save to database
        context.Domains.Add(domain);
        await context.SaveChangesAsync();

        // Publish domain events to outbox
        foreach (var domainEvent in domain.Events)
        {
            await messageContext.PublishAsync(domainEvent);
        }

        domain.ClearEvents();

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

    public class DeleteDomainHandler : IWolverineHandler
    {
        public static async Task Handle(
            DeleteDomainCommand command,
            AppDbContext context,
            IMessageContext messageContext)
        {
            var domain = await context.Domains
                .FirstOrDefaultAsync(d => d.Id == command.DomainId && d.UserId == command.UserId);

            if (domain == null)
                throw new InvalidOperationException("Domain not found or access denied");

            if (domain.IsDeleted)
                throw new InvalidOperationException("Domain is already deleted");

            domain.Delete();
            await context.SaveChangesAsync();

            foreach (var domainEvent in domain.Events)
            {
                await messageContext.PublishAsync(domainEvent);
            }

            domain.ClearEvents();
        }
    }

    public static async Task Handle(
        RestoreDomainCommand command,
        AppDbContext context,
        IMessageContext messageContext)
    {
        var domain = await context.Domains
            .FirstOrDefaultAsync(d => d.Id == command.DomainId && d.UserId == command.UserId);

        if (domain == null)
            throw new InvalidOperationException("Domain not found or access denied");

        if (!domain.IsDeleted)
            throw new InvalidOperationException("Domain is not deleted");

        // Check if domain name is available again
        var existingDomain = await context.Domains
            .FirstOrDefaultAsync(d => d.DomainName == domain.DomainName
                                      && d.UserId == command.UserId
                                      && !d.IsDeleted);

        if (existingDomain != null)
            throw new InvalidOperationException($"Cannot restore: Domain '{domain.DomainName}' already exists");

        // Restore domain
        domain.Restore();

        await context.SaveChangesAsync();

        // Publish events
        foreach (var domainEvent in domain.Events)
        {
            await messageContext.PublishAsync(domainEvent);
        }

        domain.ClearEvents();
    }

    public static async Task Handle(
        UpdateDomainNameCommand command,
        AppDbContext context,
        IMessageContext messageContext)
    {
        // Find domain
        var domain = await context.Domains
            .FirstOrDefaultAsync(d => d.Id == command.DomainId
                                      && d.UserId == command.UserId
                                      && !d.IsDeleted);

        if (domain == null)
            throw new InvalidOperationException("Domain not found or access denied");

        // Check if new domain name already exists
        if (domain.DomainName != command.NewDomainName)
        {
            var existingDomain = await context.Domains
                .FirstOrDefaultAsync(d => d.DomainName == command.NewDomainName
                                          && d.UserId == command.UserId
                                          && !d.IsDeleted);

            if (existingDomain != null)
                throw new InvalidOperationException($"Domain '{command.NewDomainName}' already exists for this user");
        }

        // Update domain
        domain.UpdateDomainName(command.NewDomainName);

        // Save changes
        await context.SaveChangesAsync();

        // Publish events
        foreach (var domainEvent in domain.Events)
        {
            await messageContext.PublishAsync(domainEvent);
        }

        domain.ClearEvents();
    }
}
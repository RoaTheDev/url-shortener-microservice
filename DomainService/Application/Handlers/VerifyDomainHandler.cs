using DomainService.Application.Commands;
using DomainService.Application.Dto.Response;
using DomainService.Infra.Persistent;
using Microsoft.EntityFrameworkCore;
using Wolverine;
using Wolverine.Attributes;
using Wolverine.EntityFrameworkCore;

namespace DomainService.Application.Handlers;

public class VerifyDomainHandler : IWolverineHandler
{
    [Transactional]
    public static async Task<VerifyDomainResult> Handle(
        VerifyDomainCommand command,
        IDbContextOutbox<AppDbContext> context)
    {
        var domain = await context.DbContext.Domains
            .FirstOrDefaultAsync(d => d.Id == command.DomainId
                                      && d.UserId == command.UserId
                                      && !d.IsDeleted);

        if (domain == null)
            throw new InvalidOperationException("Domain not found or access denied");

        var isValid = domain.Verify(command.VerificationToken);
        if (isValid)
        {
            await context.PublishAsync(domain.Events);
            domain.ClearEvents();
            await context.SaveChangesAndFlushMessagesAsync();
        }

        return new VerifyDomainResult(isValid);
    }
}
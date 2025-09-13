using System.ComponentModel.DataAnnotations;

namespace DomainService.Application.Commands;

public record CreateDomainCommand(
    [RegularExpression(@"^(?:[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z0-9][a-z0-9-]{0,61}[a-z0-9]$",
        ErrorMessage = "Domain name must be a valid format.")]
    string DomainName,
    string UserId);

public record UpdateDomainNameCommand(
    Guid DomainId,
    [RegularExpression(@"^(?:[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z0-9][a-z0-9-]{0,61}[a-z0-9]$",
        ErrorMessage = "Domain name must be a valid format.")]
    string NewDomainName,
    string UserId);

public record DeleteDomainCommand(Guid DomainId, string UserId);

public record RestoreDomainCommand(Guid DomainId, string UserId);

public record VerifyDomainCommand(Guid DomainId, string VerificationToken, string UserId);
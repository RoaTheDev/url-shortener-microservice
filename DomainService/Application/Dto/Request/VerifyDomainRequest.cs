using System.ComponentModel.DataAnnotations;

namespace DomainService.Application.Dto.Request;

public record VerifyDomainRequest(
    [Required] string VerificationToken,
    [Required] string UserId
);
using System.ComponentModel.DataAnnotations;

namespace DomainService.Application.Dto.Request;

public class UpdateDomainNameRequest
{
    [RegularExpression(@"^(?:[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z0-9][a-z0-9-]{0,61}[a-z0-9]$",
        ErrorMessage = "Domain name must be a valid format.")]
    [Required]
    public string NewDomainName { get; set; } = null!;

    [Required] public string UserId { get; set; } = null!;
}
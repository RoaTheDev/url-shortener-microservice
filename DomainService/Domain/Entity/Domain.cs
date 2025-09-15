using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using SharedContracts.Domain;
namespace DomainService.Domain.Entity;

public class Domain : DomainEvent
{
    public Guid Id { get; init; }

   
    public string DomainName { get; private set; } = null!;

    public string UserId { get; private set; } = null!;

    public bool IsVerified { get; private set; }
    public string VerificationToken { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsDeleted { get; private set; }

    private Domain()
    {
    }

    public static Domain Create(string domainName, string userId)
    {
        if (string.IsNullOrWhiteSpace(domainName))
            throw new ArgumentException("Domain name cannot be empty", nameof(domainName));

        var domain = new Domain
        {
            Id = Guid.CreateVersion7(),
            DomainName = domainName,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            IsVerified = false,
            VerificationToken = GenerateRandomToken()
        };
        domain._domainEvents.Add(new DomainCreatedEvent(domain.Id, domain.DomainName, domain.UserId));
        return domain;
    }

    public void UpdateDomainName(string newDomainName)
    {
        if (string.IsNullOrWhiteSpace(newDomainName))
            throw new ArgumentException("Domain name cannot be empty", nameof(newDomainName));

        if (newDomainName == DomainName)
            return;

        DomainName = newDomainName;
        UpdatedAt = DateTime.UtcNow;

        _domainEvents.Add(new DomainNameUpdatedEvent(Id, DomainName, UserId));
    }

    public void Delete()
    {
        if (IsDeleted) return;
        IsDeleted = true;
        IsVerified = false;
        UpdatedAt = DateTime.UtcNow;
        _domainEvents.Add(new DomainDeletedEvent(Id, UserId));
    }

    public void Restore()
    {
        if (!IsDeleted) return;
        IsDeleted = false;
        VerificationToken = GenerateRandomToken();
        _domainEvents.Add(new DomainRestoredEvent(Id, UserId));
    }

    public bool Verify(string verifyToken)
    {
        if (IsVerified) return true;
        if (IsDeleted) return false;
        if (string.IsNullOrWhiteSpace(verifyToken)) return false;

        if (verifyToken == VerificationToken)
        {
            IsVerified = true;
            UpdatedAt = DateTime.UtcNow;
            _domainEvents.Add(new DomainVerificationEvent(Id, DomainName, UserId));
            return true;
        }

        return false;
    }

    private static string GenerateRandomToken()
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] tokenData = new byte[32];
        rng.GetBytes(tokenData);
        return Convert.ToBase64String(tokenData)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}
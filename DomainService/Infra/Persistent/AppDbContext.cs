using Microsoft.EntityFrameworkCore;

namespace DomainService.Infra.Persistent;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Domain.Entity.Domain> Domains { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Domain.Entity.Domain>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.DomainName)
                .HasColumnName("domain_name")
                .HasMaxLength(150);
            e.Property(d => d.IsDeleted).HasColumnName("is_deleted");
            e.Property(d => d.VerificationToken).HasColumnName("verification_token").HasMaxLength(100);
            e.Property(d => d.UserId).HasColumnName("user_id").HasMaxLength(150).IsRequired();
            e.Property(d => d.CreatedAt).HasColumnName("created_at");
            e.Property(d => d.UpdatedAt).HasColumnName("updated_at").IsRequired(false);
            e.Property(d => d.IsVerified).HasColumnName("is_verified").HasDefaultValue(false);
            e.Property(d => d.IsDeleted).HasColumnName("is_deleted").HasDefaultValue(false);

            e.Ignore(d => d.Events);
        });
        // modelBuilder.MapWolverineEnvelopeStorage();
        base.OnModelCreating(modelBuilder);
    }
}
using KayraExport.Auth.Core.Entities;
using KayraExport.Auth.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KayraExport.Auth.Infrastructure.Persistence;

public sealed class AuthDbContext
    : IdentityDbContext<
        ApplicationUser,
        IdentityRole<Guid>,
        Guid>
{
    public AuthDbContext(
        DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens =>
        Set<RefreshToken>();

    protected override void OnModelCreating(
        ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(user => user.FullName)
                .HasMaxLength(200);

            entity.HasIndex(user => user.Email)
                .IsUnique();
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(token => token.Id);

            entity.Property(token => token.Token)
                .HasMaxLength(500)
                .IsRequired();

            entity.HasIndex(token => token.Token)
                .IsUnique();

            entity.HasOne<ApplicationUser>()
                .WithMany()
                .HasForeignKey(token => token.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
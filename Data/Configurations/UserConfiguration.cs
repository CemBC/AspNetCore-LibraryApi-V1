using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Reflection.Emit;

namespace LibraryApi.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.Role)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(u => u.RefreshTokenHash)
            .IsRequired(false);

        builder.Property(u => u.RefreshTokenExpiryTime)
            .IsRequired(false);

        builder.HasOne(u => u.Member)
            .WithOne(m => m.User)
            .HasForeignKey<Member>(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
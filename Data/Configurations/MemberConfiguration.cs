using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApi.Data.Configurations;

public class MemberConfiguration : IEntityTypeConfiguration<Member>
{
    public void Configure(EntityTypeBuilder<Member> builder)
    {
        builder.HasKey(m => m.Id);


        builder.Property(m => m.FullName)
            .IsRequired()
            .HasMaxLength(150);


        builder.Property(m => m.CreatedAt)
            .IsRequired();



        builder.HasOne(m => m.User)
            .WithOne(u => u.Member)
            .HasForeignKey<Member>(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);



        builder.HasMany(m => m.Loans)
            .WithOne(l => l.Member)
            .HasForeignKey(l => l.MemberId);

        builder.HasMany(m => m.LoanRequests)
            .WithOne(l => l.Member)
            .HasForeignKey(l => l.MemberId);
    }
}
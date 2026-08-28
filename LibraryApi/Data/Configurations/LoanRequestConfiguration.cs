using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApi.Data.Configurations;

public class LoanRequestConfiguration
    : IEntityTypeConfiguration<LoanRequest>
{
    public void Configure(
        EntityTypeBuilder<LoanRequest> builder)
    {
        builder.HasKey(l => l.Id);



        builder.Property(l => l.LoanCode)
            .IsRequired()
            .HasMaxLength(50);



        builder.Property(l => l.RequestDate)
            .IsRequired();



        builder.Property(l => l.Status)
            .HasConversion<int>()
            .IsRequired();



        builder.HasOne(l => l.Book)
            .WithMany(b => b.LoanRequests)
            .HasForeignKey(l => l.BookId)
            .OnDelete(DeleteBehavior.Restrict);



        builder.HasOne(l => l.Member)
            .WithMany(m => m.LoanRequests)
            .HasForeignKey(l => l.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
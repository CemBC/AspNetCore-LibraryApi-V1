using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApi.Data.Configurations;

public class LoanExtensionRequestConfiguration
    : IEntityTypeConfiguration<LoanExtensionRequest>
{
    public void Configure(
        EntityTypeBuilder<LoanExtensionRequest> builder)
    {
        builder.HasKey(x => x.Id);


        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();



        builder.HasOne(x => x.Loan)
            .WithMany(l => l.ExtensionRequests)
            .HasForeignKey(x => x.LoanId)
            .OnDelete(DeleteBehavior.Restrict);



        builder.HasOne(x => x.Member)
            .WithMany(m => m.ExtensionRequests)
            .HasForeignKey(x => x.MemberId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
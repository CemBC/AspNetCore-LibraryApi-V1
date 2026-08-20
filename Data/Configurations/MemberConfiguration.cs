using LibraryApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApi.Data.Configurations
{
    public class MemberConfiguration : IEntityTypeConfiguration<Member>
    {
        public void Configure(EntityTypeBuilder<Member> builder)
        {
            builder.HasKey(m => m.Id);

            builder.Property(m => m.FullName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(m => m.Email)
                .IsRequired()
                .HasMaxLength(200);


            builder.HasData(
                new Member
                {
                    Id = 1,
                    FullName = "John Doe",
                    Email = "john.doe@example.com"
                },
                new Member
                {
                    Id = 2,
                    FullName = "Jane Smith",
                    Email = "jane.smith@example.com"
                }
            );
        }
    }
}
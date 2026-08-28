using LibraryApi.Models;
using LibraryApi.Models.Status;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LibraryApi.Data.Configurations
{
    public class BookConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {
            builder.HasKey(b => b.Id);

            builder.Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(b => b.Author)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(b => b.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.HasMany(b => b.LoanRequests)
                .WithOne(l => l.Book)
                .HasForeignKey(l => l.BookId);


            builder.HasData(
                new Book
                {
                    Id = 1,
                    Title = "Suç ve Ceza",
                    Author = "Fyodor Dostoyevski",
                    Status = BookStatus.Available
                },
                new Book
                {
                    Id = 2,
                    Title = "1984",
                    Author = "George Orwell",
                    Status = BookStatus.Available
                }
            );
        }
    }
}
using Microsoft.EntityFrameworkCore;
using LibraryApi.Models;
using LibraryApi.Data.Configurations;

namespace LibraryApi.Data
{
    public class LibraryDbContext : DbContext
    {
        public LibraryDbContext(DbContextOptions<LibraryDbContext> options) : base(options)
        {
        }
        public DbSet<Book> Books { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Loan> Loans { get; set; }

        public DbSet<User> Users { get; set; }  

        public DbSet<LoanRequest> LoanRequest { get; set; }

        public DbSet<LoanExtensionRequest> LoanExtensionRequest { get; set; } 

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new BookConfiguration());
            modelBuilder.ApplyConfiguration(new MemberConfiguration());
            modelBuilder.ApplyConfiguration(new LoanConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new LoanRequestConfiguration());
        }
    }
}

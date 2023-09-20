using Microsoft.EntityFrameworkCore;
using Tmm.Models;

namespace Tmm.Data
{
    public class TmmDbContext : DbContext
    {
        public TmmDbContext(DbContextOptions<TmmDbContext> options) : base(options) { }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<Address> Addresses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Customer>()
                .HasMany(c => c.Addresses)
                .WithOne(a => a.Customer)
                .HasForeignKey(a => a.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.EmailAddress)
                .IsUnique();
        }
    }
}

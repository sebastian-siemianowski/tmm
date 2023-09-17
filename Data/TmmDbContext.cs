using Microsoft.EntityFrameworkCore;
using Tmm.Models;

namespace Tmm.Data
{
    public class TmmDbContext : DbContext
    {
        public TmmDbContext(DbContextOptions<TmmDbContext> options) : base(options) { }
        
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Address> Addresses { get; set; }
    }
}

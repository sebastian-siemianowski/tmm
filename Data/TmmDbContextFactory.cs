using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Tmm.Data
{
    public class TmmDbContextFactory : IDesignTimeDbContextFactory<TmmDbContext>
    {
        public TmmDbContext CreateDbContext(string[] args)
        {
            // Build configuration
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            var config = builder.Build();

            // Retrieve the connection string
            var connectionString = config.GetConnectionString("DefaultConnection");

            // Setup DbContext options
            var optionsBuilder = new DbContextOptionsBuilder<TmmDbContext>();
            optionsBuilder.UseSqlServer(connectionString);

            return new TmmDbContext(optionsBuilder.Options);
        }
    }
}

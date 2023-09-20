using Microsoft.EntityFrameworkCore;
using Tmm.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

namespace Tmm
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<TmmDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
            });

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, TmmDbContext dbContext)
        {

            if (env.IsDevelopment())
            {
                ClearDatabase(dbContext);
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private void ClearDatabase(TmmDbContext dbContext)
        {
            // Note the order here is important because of foreign key constraints
            dbContext.Addresses.RemoveRange(dbContext.Addresses.ToList()); // Convert to list to immediately execute the query
            dbContext.Customers.RemoveRange(dbContext.Customers.ToList()); // Convert to list to immediately execute the query
            dbContext.SaveChanges();
        }
    }
}

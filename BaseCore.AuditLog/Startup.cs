using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BaseCore.Repository;
using BaseCore.Repository.EFCore;
using BaseCore.LogService;

namespace BaseCore.AuditLog
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            // SQL Server Configuration
            var connectionString = Configuration.GetConnectionString("DefaultConnection") ?? 
                                  "Server=(localdb)\\mssqllocaldb;Database=BaseCoreSales;Trusted_Connection=True;MultipleActiveResultSets=true";

            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(connectionString));

            // Generic Repository Registration
            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            // Logging Services
            services.AddScoped<ILogActionService, LogActionService>();
            services.AddScoped<ILogErrorService, LogErrorService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}

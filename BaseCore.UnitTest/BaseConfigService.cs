using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using BaseCore.Common;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using BaseCore.Repository;
using BaseCore.Repository.EFCore;

namespace BaseCore.UnitTest
{
    public class BaseConfigService
    {
        public IOptions<AppSettings> Option;
        public readonly IConfiguration ConfigurationRoot;
        public readonly IServiceProvider ServiceProvider;

        public BaseConfigService()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            ConfigurationRoot = builder.Build();
            Option = Options.Create(new AppSettings
            {
                Secret = "TestSecretKeyForAuthenticationShouldBeLongEnough"
            });

            IServiceCollection services = new ServiceCollection();
            
            // SQL Server Configuration (using In-Memory for tests or actual localdb if preferred)
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("BaseCoreTestDb"));

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<IProductRepository, ProductRepository>();
            services.AddScoped<IOrderRepository, OrderRepository>();

            ServiceProvider = services.BuildServiceProvider();
        }
    }
}

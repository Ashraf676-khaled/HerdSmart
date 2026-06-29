using Application.Common.Interfaces;
using HerdSmart.Infrastructure.Data;
using HerdSmart.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastrucre.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastrucre 
            (this IServiceCollection services
            , IConfiguration configuration
            )
        {
            var connectionString = configuration.GetConnectionString("default");
            services.AddHttpContextAccessor();
            services.AddScoped<ITenantProvider,TenantProvider>();
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
            return services;
        }
    }
}

using Application.Common.Interfaces;
using HerdSmart.Domain.Entities;
using HerdSmart.Infrastructure.Data;
using HerdSmart.Infrastructure.Services;
using Infrastrucre.Settings;
using Infrastructure.Services.BackgroundJobs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Infrastrucre.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastrucre
            (this IServiceCollection services
            , IConfiguration configuration
            )
        {
            //connection to db
            var connectionString = configuration.GetConnectionString("default");
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
            //connection to redis
            var redisConnectionString = configuration.GetConnectionString("Redis");
            services.AddStackExchangeRedisCache(
                options =>
                {
                    options.Configuration = redisConnectionString;
                    options.InstanceName = "HerdSmart_";
                });
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            //jwt
            services.Configure<Jwt>(configuration.GetSection("JWT"));
            services.AddScoped<IJwtService, JwtService>();
            //Identity
            services.AddIdentity<AppUser, IdentityRole <Guid>>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();
            // JWT Authentication

            var jwtSettings = configuration.GetSection("JWT").Get<Jwt>()
                ?? throw new InvalidOperationException("JWT settings are missing in appsettings.json!");

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key))
                };
            });
            //Background Jobs
            services.AddScoped<MarkOverdueVaccinationsJob>();
            services.AddScoped<AutoGenerateVaccinationSchedulesJob>();
            //For MultiTenancy
            services.AddHttpContextAccessor();
            services.AddScoped<ITenantProvider,TenantProvider>();
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());
            return services;
        }
    }
}

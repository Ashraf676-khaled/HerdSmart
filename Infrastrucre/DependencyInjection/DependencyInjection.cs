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
        public static IServiceCollection AddInfrastrucre(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            // 1. Connection to DB
            var connectionString = configuration.GetConnectionString("default");
            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));

            // 2. Connection to Redis
            var redisConnectionString = configuration.GetConnectionString("Redis");
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "HerdSmart_";
            });

            services.AddScoped<IRefreshTokenService, RefreshTokenService>();

            // 3. JWT Settings & Services
            services.Configure<Jwt>(configuration.GetSection("JWT"));
            services.AddScoped<IJwtService, JwtService>();

            // 4. Identity
            services.AddIdentity<AppUser, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            // 5. JWT Authentication Setup
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
                // استقبال الـ Token من الـ Query String للـ SignalR Hubs
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;

                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };

                options.SaveToken = true;
                options.RequireHttpsMetadata = false; // للتطوير فقط، في الـ Production بنخليها true
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

            // 6. Background Jobs
            services.AddScoped<MarkOverdueVaccinationsJob>();
            services.AddScoped<AutoGenerateVaccinationSchedulesJob>();

            // 7. For MultiTenancy
            services.AddHttpContextAccessor();
            services.AddScoped<ITenantProvider, TenantProvider>();
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

            return services;
        }
    }
}
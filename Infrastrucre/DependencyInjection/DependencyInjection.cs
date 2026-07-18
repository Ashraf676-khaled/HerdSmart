using Application.Common.Interfaces;
using Application.Features.Cattle.HealthChecks;
using Application.Features.HealthLogs.HealthChecks;
using Application.Features.MilkLogs.HealthChecks;
using Application.Features.Telemetry.HealthChecks;
using Application.Features.Vaccinations.HealthChecks;
using HerdSmart.Domain.Entities;
using HerdSmart.Infrastructure.Data;
using HerdSmart.Infrastructure.Services;
using Infrastrucre.Settings;
using Infrastructure.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
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
            services.AddSingleton<IConnectionMultiplexer>(sp =>
             ConnectionMultiplexer.Connect("localhost:6379")); 

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
            // HealthCheck
            var healthChecksBuilder = services.AddHealthChecks()
                .AddSqlServer(
                    configuration.GetConnectionString("default")!,
                    name: "database",
                    tags: ["ready"])
                .AddHangfire(options =>
                {
                    options.MinimumAvailableServers = 1;
                }, name: "hangfire", tags: ["ready"]);

            var redisConnection = configuration.GetConnectionString("Redis");
            if (!string.IsNullOrEmpty(redisConnection))
            {
                healthChecksBuilder.AddRedis(redisConnection, name: "redis", tags: ["ready"]);
            }

            services.AddHealthChecks()
              .AddCheck<TelemetryIngestionHealthCheck>("telemetry-ingestion", tags: ["business"])
              .AddCheck<HangfireJobsHealthCheck>("hangfire-jobs", tags: ["business"])
              .AddCheck<AuthTokenStoreHealthCheck>("auth-token-store", tags: ["business"])
              .AddCheck<CattleDataHealthCheck>("cattle-data", tags: ["business"])
              .AddCheck<HealthLogDataHealthCheck>("health-log-data", tags: ["business"])
              .AddCheck<MilkProductionHealthCheck>("milk-production-data", tags: ["business"])
              .AddCheck<VaccinationScheduleHealthCheck>("vaccination-schedule-data", tags: ["business"]);
            


            // 7. For MultiTenancy
            services.AddHttpContextAccessor();
            services.AddScoped<ITenantProvider, TenantProvider>();
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

            return services;
        }
    }
}
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

            // 2. إعداد الاتصال بـ Upstash Redis
            var redisConnectionString = configuration.GetConnectionString("Redis");

            if (!string.IsNullOrWhiteSpace(redisConnectionString))
            {
                var options = ConfigurationOptions.Parse(redisConnectionString);
                options.Ssl = true; // إجباري لـ Upstash
                options.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13;
                options.AbortOnConnectFail = false;
                options.ConnectTimeout = 15000;
                options.SyncTimeout = 15000;
                options.KeepAlive = 30;

                // إنشاء وتداول اتصال واحد ثابت
                var multiplexer = ConnectionMultiplexer.Connect(options);
                services.AddSingleton<IConnectionMultiplexer>(multiplexer);

                // ربط الـ Distributed Cache بنفس الـ Multiplexer
                services.AddStackExchangeRedisCache(opt =>
                {
                    opt.ConnectionMultiplexerFactory = () => Task.FromResult<IConnectionMultiplexer>(multiplexer);
                    opt.InstanceName = "HerdSmart_";
                });
            }
            else
            {
                services.AddDistributedMemoryCache();
            }

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

            // 6. HealthChecks
            var healthChecksBuilder = services.AddHealthChecks()
                .AddSqlServer(
                    configuration.GetConnectionString("default")!,
                    name: "database",
                    tags: ["ready"])
                .AddHangfire(options =>
                {
                    options.MinimumAvailableServers = 1;
                }, name: "hangfire", tags: ["ready"]);

            if (!string.IsNullOrEmpty(redisConnectionString))
            {
                // استخدام الـ Singleton Connection المظبوط في الـ HealthCheck
                healthChecksBuilder.AddRedis(sp => sp.GetRequiredService<IConnectionMultiplexer>(), name: "redis", tags: ["ready"]);
            }

            services.AddHealthChecks()
              .AddCheck<TelemetryIngestionHealthCheck>("telemetry-ingestion", tags: ["business"])
              .AddCheck<HangfireJobsHealthCheck>("hangfire-jobs", tags: ["business"])
              .AddCheck<AuthTokenStoreHealthCheck>("auth-token-store", tags: ["business"])
              .AddCheck<CattleDataHealthCheck>("cattle-data", tags: ["business"])
              .AddCheck<HealthLogDataHealthCheck>("health-log-data", tags: ["business"])
              .AddCheck<MilkProductionHealthCheck>("milk-production-data", tags: ["business"])
              .AddCheck<VaccinationScheduleHealthCheck>("vaccination-schedule-data", tags: ["business"]);

            // 7. For MultiTenancy & Services
            services.AddHttpContextAccessor();
            services.AddScoped<ITenantProvider, TenantProvider>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

            return services;
        }
    }
}
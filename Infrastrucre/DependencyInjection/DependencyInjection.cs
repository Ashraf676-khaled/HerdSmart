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

            // 1. تسجيل خدمة IRefreshTokenService
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();

            // 2. إعداد الاتصال بـ Upstash Redis
            var redisConnectionString = configuration.GetConnectionString("Redis");

            if (!string.IsNullOrWhiteSpace(redisConnectionString))
            {
                // تحليل نص الاتصال وتأكيد خيارات التشفير الخاصة بـ Upstash
                var options = ConfigurationOptions.Parse(redisConnectionString);
                options.Ssl = true; // إجباري لـ Upstash
                options.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13;
                options.AbortOnConnectFail = false; // لمنع الاستثناء عند التحميل المبدئي
                options.ConnectTimeout = 10000; // زيادة مهلة الاتصال لـ 10 ثوانٍ
                options.SyncTimeout = 10000;

                // تسجيل Distributed Cache
                services.AddStackExchangeRedisCache(opt =>
                {
                    opt.ConfigurationOptions = options;
                    opt.InstanceName = "HerdSmart_";
                });

                // تسجيل IConnectionMultiplexer
                services.AddSingleton<IConnectionMultiplexer>(sp =>
                    ConnectionMultiplexer.Connect(options));
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
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<AppDbContext>());

            return services;
        }
    }
}
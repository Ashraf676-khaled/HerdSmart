using Api.Infrastructure;
using API.Middleware;
using Hangfire;
using Hangfire.SqlServer;
using Infrastrucre.DependencyInjection;
using Infrastructure.Services.BackgroundJobs;
using Infrastructure.Services.BackgroundJobs.Extensions;
using Scalar.AspNetCore;
using Serilog;
using System.Text.Json.Serialization;
using Web.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHangfireServer();

//Controller
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters
            .Add(new JsonStringEnumConverter());
    }); builder.Services.AddDistributedMemoryCache();

//Serilog
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/herdsmart-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Host.UseSerilog();

//Dependincy Injection
builder.Services.AddApplication();
builder.Services.AddInfrastrucre(builder.Configuration);

//Signalir
builder.Services.AddSignalR();

//Hangfire
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("default")
, new SqlServerStorageOptions
{
    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
    QueuePollInterval = TimeSpan.Zero,
    UseRecommendedIsolationLevel = true,
    DisableGlobalLocks = true
}));

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); 

    app.MapScalarApiReference(options =>
    {
        options.WithTitle("HerdSmart API Reference")
               .WithTheme(ScalarTheme.DeepSpace)
               .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}
app.UseHttpsRedirection(); 

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() }
});
app.RegisterRecurringJobs();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");

app.Run();
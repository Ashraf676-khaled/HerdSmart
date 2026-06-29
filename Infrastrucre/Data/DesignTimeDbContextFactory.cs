using Application.Common.Interfaces;
using HerdSmart.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
       .UseSqlServer("Server=.;Database=HerdSmart;Trusted_Connection=True;TrustServerCertificate=True")
       .Options;


        return new AppDbContext(options, new FakeTenantProvider());
    }
}

public class FakeTenantProvider : ITenantProvider
{
    public Ulid GetTenantId() => Ulid.NewUlid();
    public Guid GetUserId() => Guid.Empty;
    public string GetUserRole() => "Owner";
    public bool IsAuthenticated() => false;
}
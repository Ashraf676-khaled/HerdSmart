// Api/Infrastructure/HangfireAuthorizationFilter.cs
using Hangfire.Dashboard;

namespace Api.Infrastructure;

public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        //return httpContext.User.Identity?.IsAuthenticated == true &&
        //       httpContext.User.IsInRole("Owner");
        return true;
    }
}
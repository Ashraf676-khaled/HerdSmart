// Application/Extensions/QueryableExtensions.cs
using HerdSmart.Domain.Entities;

public static class QueryableExtensions
{
    public static IQueryable<T> ForTenant<T>(this IQueryable<T> query, Ulid tenantId)
        where T : BaseEntity
    {
        return query.Where(e => e.Id == tenantId);
    }
}
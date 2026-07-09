using Microsoft.EntityFrameworkCore;

public static class PaginationHelper
{
    public static async Task<PaginatedResult<T>> AsPaginationAsync<T>(this IQueryable<T> source, int pageNumber, int pageSize, CancellationToken cancellationToken = new CancellationToken())
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException("pageNumber", "The page number should be greater than or equal to 1.");
        }

        int numberToSkip = (pageNumber - 1) * pageSize;
        var count = await source.CountAsync(cancellationToken);
        var data = await source.Skip(numberToSkip).Take(pageSize).ToListAsync(cancellationToken);

        return new PaginatedResult<T>(data, count, pageNumber, pageSize);
    }

    public static async Task<PaginatedResult<T>> AsPaginationListAsync<T>(this List<T> source, int pageNumber, int pageSize, CancellationToken cancellationToken = new CancellationToken())
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException("pageNumber", "The page number should be greater than or equal to 1.");
        }

        int numberToSkip = (pageNumber - 1) * pageSize;
        var count = source.Count();
        var data = source.Skip(numberToSkip).Take(pageSize).ToList();

        return new PaginatedResult<T>(data, count, pageNumber, pageSize);
    }

    public static async Task<PaginatedResult<TD>> AsPaginationAsync<T, TD>(this IQueryable<T> source, int pageNumber, int pageSize, Func<IEnumerable<T>, IEnumerable<TD>> mapFunction, CancellationToken cancellationToken = new CancellationToken())
    {
        var fun = await AsPaginationAsync(source, pageNumber, pageSize, cancellationToken);
        var mappeddata = mapFunction(fun.Items);

        return new PaginatedResult<TD>(mappeddata, fun.TotalCount, fun.Page, fun.PageSize);
    }

    public static async Task<PaginatedResult<TD>> PaginationListAsync<T, TD>(this List<T> source, int pageNumber, int pageSize, Func<IEnumerable<T>, IEnumerable<TD>> mapFunction, CancellationToken cancellationToken = new CancellationToken())
    {
        var fun = await AsPaginationListAsync(source, pageNumber, pageSize, cancellationToken);
        var mappeddata = mapFunction(fun.Items);

        return new PaginatedResult<TD>(mappeddata, fun.TotalCount, fun.Page, fun.PageSize);
    }

    public static async Task<PaginatedResult<T>> AsPaginationAsync<T>(this IEnumerable<T> source, int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1)
        {
            throw new ArgumentOutOfRangeException("pageNumber", "The page number should be greater than or equal to 1.");
        }

        var totalItems = source.Count();
        var items = source.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

        return new PaginatedResult<T>(items, totalItems, pageNumber, pageSize);
    }
}
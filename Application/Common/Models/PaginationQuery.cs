// Common/Models/PaginationQuery.cs
public record PaginationQuery(
    int Page = 1,
    int PageSize = 10);
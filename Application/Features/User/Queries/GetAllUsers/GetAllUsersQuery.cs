using Application.Features.User.Dtos;
using MediatR;

public record GetUsersQuery(
    int Page = 1,
    int PageSize = 10,
    string? Search = null) : IRequest<PaginatedResult<UserResponse>>;
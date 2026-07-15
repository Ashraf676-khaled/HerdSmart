
using Application.Features.Vaccines;
using MediatR;

public record GetAllVaccinesQuery(
    int Page = 1,
    int PageSize = 10) : IRequest<PaginatedResult<VaccineResponse>>;
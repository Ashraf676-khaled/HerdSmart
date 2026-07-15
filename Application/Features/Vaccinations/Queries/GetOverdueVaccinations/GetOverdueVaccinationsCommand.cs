
using Application.Features.Vaccinations;
using MediatR;

public record GetOverdueVaccinationsQuery(
    int Page = 1,
    int PageSize = 10) : IRequest<PaginatedResult<VaccinationScheduleResponse>>;
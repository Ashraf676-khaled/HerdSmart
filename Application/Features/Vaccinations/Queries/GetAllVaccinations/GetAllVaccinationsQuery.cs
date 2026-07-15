
using Application.Features.Vaccinations;
using HerdSmart.Domain.Enums;
using MediatR;

public record GetAllVaccinationsQuery(
    int Page = 1,
    int PageSize = 10,
    VaccinationStatus? Status = null,
    DateTimeOffset? From = null,
    DateTimeOffset? To = null) : IRequest<PaginatedResult<VaccinationScheduleResponse>>;
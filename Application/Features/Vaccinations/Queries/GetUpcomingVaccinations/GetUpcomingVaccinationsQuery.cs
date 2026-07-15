
using Application.Features.Vaccinations;
using MediatR;

public record GetUpcomingVaccinationsQuery(int Days = 7)
    : IRequest<IEnumerable<VaccinationScheduleResponse>>;
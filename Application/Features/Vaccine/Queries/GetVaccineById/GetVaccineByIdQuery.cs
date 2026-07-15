using Application.Features.Vaccines;
using MediatR;

public record GetVaccineByIdQuery(Ulid Id) : IRequest<VaccineResponse>;

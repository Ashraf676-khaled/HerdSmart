using Application.Features.Cattle.Dtos;
using MediatR;

namespace Application.Features.Cattle.Commands.UpdateCattle
{
    public record UpdateCattleCommand(
    Ulid Id,
    string TagNumber,
    string Breed,
    DateTimeOffset BirthDate,
    string? FatherTagNumber,
    string? MotherTagNumber) : IRequest<CattleResponse>;
}

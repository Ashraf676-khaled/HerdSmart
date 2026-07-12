// Application/Features/Cattle/Commands/CreateCattle/CreateCattleCommand.cs
using Application.Features.Cattle.Dtos;
using HerdSmart.Domain.Enums;
using MediatR;

namespace Application.Features.Cattle.Commands.CreateCattle;

public sealed record CreateCattleCommand(
    string TagNumber,
    string Breed,
    Gender Gender,
    DateTimeOffset BirthDate,
    string? FatherTagNumber,
    string? MotherTagNumber) : IRequest<CattleResponse>;
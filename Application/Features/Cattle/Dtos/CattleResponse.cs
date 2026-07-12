// Application/Features/Cattle/Dtos/CattleResponse.cs
using HerdSmart.Domain.Enums;

namespace Application.Features.Cattle.Dtos;

public record CattleResponse(
    Ulid Id,
    string TagNumber,
    string Breed,
    Gender Gender,
    DateTimeOffset BirthDate,
    CattleStatus Status,
    string? FatherTagNumber,
    string? MotherTagNumber,
    DateTimeOffset CreatedAt);
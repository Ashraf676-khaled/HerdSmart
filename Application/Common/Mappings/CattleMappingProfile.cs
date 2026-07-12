// Application/Common/Mappings/CattleMappingProfile.cs
using Application.Features.Cattle.Commands.CreateCattle;
using Application.Features.Cattle.Dtos;
using AutoMapper;
using HerdSmart.Domain.Entities;

namespace Application.Common.Mappings;

public class CattleMappingProfile : Profile
{
    public CattleMappingProfile()
    {
        CreateMap<CreateCattleCommand, Cattle>();
        CreateMap<Cattle, CattleResponse>();
    }
}
// Application/Common/Mappings/MilkProductionMappingProfile.cs
using Application.Features.MilkLogs.Commands.CreateMilkLog;
using AutoMapper;
using HerdSmart.Domain.Entities;

namespace Application.Common.Mappings;

public class MilkProductionMappingProfile : Profile
{
    public MilkProductionMappingProfile()
    {
        CreateMap<CreateMilkLogCommand, MilkProductionLog>()
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.Cattle, opt => opt.Ignore())
            .ForMember(dest => dest.LoggedAt, opt => opt.MapFrom(src =>
                src.LoggedAt ?? DateTimeOffset.UtcNow));
    }
}
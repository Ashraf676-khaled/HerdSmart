// Application/Common/Mappings/HealthLogMappingProfile.cs
using Application.Features.HealthLog.Commands.CreateHealthLog;
using AutoMapper;
using HerdSmart.Domain.Entities;

public class HealthLogMappingProfile : Profile
{
    public HealthLogMappingProfile()
    {
        CreateMap<CreateHealthLogCommand, HealthLog>()
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.Cattle, opt => opt.Ignore());
    }
}
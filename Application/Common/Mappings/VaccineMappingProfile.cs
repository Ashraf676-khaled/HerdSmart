// Application/Common/Mappings/VaccineMappingProfile.cs
using Application.Features.Vaccines;
using Application.Features.Vaccines.Commands.CreateVaccine;
using AutoMapper;
using HerdSmart.Domain.Entities;

namespace Application.Common.Mappings;

public class VaccineMappingProfile : Profile
{
    public VaccineMappingProfile()
    {
        CreateMap<CreateVaccineCommand, Vaccine>()
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.Schedules, opt => opt.Ignore());

        CreateMap<Vaccine, VaccineResponse>();
    }
}
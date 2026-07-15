// Application/Common/Mappings/VaccinationsMappingProfile.cs
using Application.Features.Vaccinations;
using Application.Features.Vaccinations.Commands.CreateVaccinationSchedule;
using AutoMapper;
using HerdSmart.Domain.Entities;

namespace Application.Common.Mappings;

public class VaccinationsMappingProfile : Profile
{
    public VaccinationsMappingProfile()
    {
        CreateMap<CreateVaccinationScheduleCommand, VaccinationSchedule>()
            .ForMember(dest => dest.TenantId, opt => opt.Ignore())
            .ForMember(dest => dest.Cattle, opt => opt.Ignore())
            .ForMember(dest => dest.Vaccine, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.AdministeredDate, opt => opt.Ignore());

        CreateMap<VaccinationSchedule, VaccinationScheduleResponse>()
            .ForCtorParam("CattleTagNumber", opt => opt.MapFrom(s => s.Cattle.TagNumber))
            .ForCtorParam("VaccineName", opt => opt.MapFrom(s => s.Vaccine.Name));
    }
}
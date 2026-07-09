using Application.Features.Auth.Commands.Register;
using Application.Features.User.Commands.CreateUser;
using AutoMapper;
using HerdSmart.Domain.Entities;
using HerdSmart.Domain.Enums;

namespace Application.Common.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // RegisterCommand → Tenant + AppUser
            CreateMap<RegisterCommand, Tenant>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.FarmName))
                .ForMember(dest => dest.Plan, opt => opt.MapFrom(_=>SubscriptionPlan.Basic));

            CreateMap<RegisterCommand, AppUser>()
                .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.OwnerName))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));

            // CreateUserCommand → AppUser (لـ Vet/Worker)
            CreateMap<CreateUserCommand, AppUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Email));
        }
    }
}
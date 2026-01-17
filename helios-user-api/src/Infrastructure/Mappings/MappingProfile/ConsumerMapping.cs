using AutoMapper;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Infrastructure.Mappings.MappingProfile
{
    public class ConsumerMapping : Profile
    {
        public ConsumerMapping()
        {
            CreateMap<ConsumerDto, ConsumerModel>().ReverseMap();
            CreateMap<ConsumerModel, GetConsumerResponseDto>()
                .ForPath(x => x.Consumer.ConsumerId, y => y.MapFrom(z => z.ConsumerId))
                .ForPath(x => x.Consumer.PersonId, y => y.MapFrom(z => z.PersonId))
                .ForPath(x => x.Consumer.TenantCode, y => y.MapFrom(z => z.TenantCode))
                .ForPath(x => x.Consumer.ConsumerCode, y => y.MapFrom(z => z.ConsumerCode))
                .ForPath(x => x.Consumer.RegistrationTs, y => y.MapFrom(z => z.RegistrationTs))
                .ForPath(x => x.Consumer.EligibleStartTs, y => y.MapFrom(z => z.EligibleStartTs))
                .ForPath(x => x.Consumer.EligibleEndTs, y => y.MapFrom(z => z.EligibleEndTs))
                .ForPath(x => x.Consumer.CreateTs, y => y.MapFrom(z => z.CreateTs))
                .ForPath(x => x.Consumer.UpdateTs, y => y.MapFrom(z => z.UpdateTs))
                .ForPath(x => x.Consumer.CreateUser, y => y.MapFrom(z => z.CreateUser))
                .ForPath(x => x.Consumer.UpdateUser, y => y.MapFrom(z => z.UpdateUser))
                .ForPath(x => x.Consumer.DeleteNbr, y => y.MapFrom(z => z.DeleteNbr))
                .ForPath(x => x.Consumer.Registered, y => y.MapFrom(z => z.Registered))
                .ForPath(x => x.Consumer.Eligible, y => y.MapFrom(z => z.Eligible))
                .ForPath(x => x.Consumer.MemberNbr, y => y.MapFrom(z => z.MemberNbr))
                .ForPath(x => x.Consumer.SubscriberMemberNbr, y => y.MapFrom(z => z.SubscriberMemberNbr))
                .ForPath(x => x.Consumer.ConsumerAttribute, y => y.MapFrom(z => z.ConsumerAttribute))
                .ForPath(x => x.Consumer.AnonymousCode, y => y.MapFrom(z => z.AnonymousCode))
                .ForPath(x => x.Consumer.EnrollmentStatus, y => y.MapFrom(z => z.EnrollmentStatus))
                .ForPath(x => x.Consumer.EnrollmentStatusSource, y => y.MapFrom(z => z.EnrollmentStatusSource))
                .ForPath(x => x.Consumer.OnBoardingState, y => y.MapFrom(z => z.OnBoardingState))
                .ForPath(x => x.Consumer.AgreementStatus, y => y.MapFrom(z => z.AgreementStatus))
                .ForPath(x => x.Consumer.AgreementFileName, y => y.MapFrom(z => z.AgreementFileName))
                .ReverseMap();
            CreateMap<ConsumerModel, GetConsumerByMemIdResponseDto>()
                .ForPath(x => x.Consumer, y => y.MapFrom(z => z))
                .ReverseMap();
            CreateMap<ConsumerDto, ConsumerRequestDto>().ReverseMap();
        }
    }
}
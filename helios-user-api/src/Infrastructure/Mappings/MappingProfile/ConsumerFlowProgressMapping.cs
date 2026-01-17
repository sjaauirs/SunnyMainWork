using AutoMapper;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Infrastructure.Mappings.MappingProfile
{
    public class ConsumerFlowProgressMapping : Profile
    {
        public ConsumerFlowProgressMapping()
        {
            CreateMap<ConsumerFlowProgressDto, ConsumerFlowProgressModel>().ReverseMap();
            CreateMap<UpdateFlowStatusRequestDto, ConsumerFlowProgressModel>().ReverseMap();
            CreateMap<ConsumerFlowProgressModel, ConsumerFlowProgressResponseDto>()
                .ForPath(x => x.ConsumerFlowProgress.Pk, y => y.MapFrom(z => z.Pk))
                .ForPath(x => x.ConsumerFlowProgress.ConsumerCode, y => y.MapFrom(z => z.ConsumerCode))
                .ForPath(x => x.ConsumerFlowProgress.CohortCode, y => y.MapFrom(z => z.CohortCode))
                .ForPath(x => x.ConsumerFlowProgress.TenantCode, y => y.MapFrom(z => z.TenantCode))
                .ForPath(x => x.ConsumerFlowProgress.FlowFk, y => y.MapFrom(z => z.FlowFk))
                .ForPath(x => x.ConsumerFlowProgress.FlowStepPk, y => y.MapFrom(z => z.FlowStepPk))
                .ForPath(x => x.ConsumerFlowProgress.Status, y => y.MapFrom(z => z.Status))
                .ForPath(x => x.ConsumerFlowProgress.VersionNbr, y => y.MapFrom(z => z.VersionNbr))
                .ForPath(x => x.ConsumerFlowProgress.CreateTs, y => y.MapFrom(z => z.CreateTs))
                .ForPath(x => x.ConsumerFlowProgress.UpdateTs, y => y.MapFrom(z => z.UpdateTs))
                .ForPath(x => x.ConsumerFlowProgress.CreateUser, y => y.MapFrom(z => z.CreateUser))
                .ForPath(x => x.ConsumerFlowProgress.UpdateUser, y => y.MapFrom(z => z.UpdateUser))
                .ForPath(x => x.ConsumerFlowProgress.DeleteNbr, y => y.MapFrom(z => z.DeleteNbr))
                .ReverseMap();
            CreateMap<ConsumerModel, GetConsumerByMemIdResponseDto>()
                .ForPath(x => x.Consumer, y => y.MapFrom(z => z))
                .ReverseMap();
            CreateMap<ConsumerDto, ConsumerRequestDto>().ReverseMap();
        }
    }
}
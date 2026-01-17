using AutoMapper;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;


namespace SunnyRewards.Helios.ETL.Infrastructure.Profiles
{
    public class BatchJobDetailReportMappingProfile : Profile
    {
        public BatchJobDetailReportMappingProfile()
        {
            CreateMap<BatchJobDetailReportDto, BatchJobDetailReportModel>()
            .ForMember(dest => dest.BatchJobDetailReportId, opt => opt.MapFrom(src=> src.BatchJobDetailReportId))
            .ForMember(dest => dest.BatchJobReportId, opt => opt.MapFrom(src => src.BatchJobReportId))
            .ForMember(dest => dest.FileNum, opt => opt.MapFrom(src => src.FileNum))
            .ForMember(dest => dest.RecordNum, opt => opt.MapFrom(src => src.RecordNum))
            .ForMember(dest => dest.RecordResultJson, opt => opt.MapFrom(src => src.RecordResultJson))
            .ForMember(dest => dest.CreateTs, opt => opt.MapFrom(_ => DateTime.Now))
            .ForMember(dest => dest.DeleteNbr, opt => opt.MapFrom(_ => 0))
            .ForMember(dest => dest.CreateUser, opt => opt.MapFrom(_ => Constant.CreateUserAsETL)).ReverseMap();
        }
    }
}

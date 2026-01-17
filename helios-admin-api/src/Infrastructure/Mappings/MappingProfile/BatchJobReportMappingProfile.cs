using AutoMapper;
using SunnyRewards.Helios.Admin.Core.Domain.Constants;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using System;


namespace SunnyRewards.Helios.ETL.Infrastructure.Profiles
{
    public class BatchJobReportMappingProfile : Profile
    {
        public BatchJobReportMappingProfile()
        {
            CreateMap<BatchJobReportDto, BatchJobReportModel>()
            .ForMember(dest => dest.BatchJobReportId, opt => opt.MapFrom(src => src.BatchJobReportId))
            .ForMember(dest => dest.BatchJobReportCode, opt => opt.MapFrom(src => src.BatchJobReportCode))
            .ForMember(dest => dest.JobResultJson, opt => opt.MapFrom(src => src.JobResultJson))
            .ForMember(dest => dest.ValidationJson, opt => opt.MapFrom(src => src.ValidationJson))
            .ForMember(dest => dest.JobType, opt => opt.MapFrom(src => src.JobType))
            .ForMember(dest => dest.CreateTs, opt => opt.MapFrom(_ => DateTime.Now))
            .ForMember(dest => dest.DeleteNbr, opt => opt.MapFrom(_ => 0))
            .ForMember(dest => dest.CreateUser, opt => opt.MapFrom(_ => Constant.CreateUserAsETL)).ReverseMap();
        }
    }
}

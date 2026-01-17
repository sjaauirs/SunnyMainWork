using AutoMapper;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile
{
    public class TriviaRequestMapping : Profile
    {
        public TriviaRequestMapping()
        {
            CreateMap<Trivia, TriviaModel>().ReverseMap();

            CreateMap<TriviaQuestionRequestDto, TriviaQuestionDto>().ReverseMap();
            CreateMap<TriviaQuestionData, TriviaQuestionDto>().ReverseMap();
            CreateMap<TriviaQuestionRequestDto, TriviaQuestionModel>()
                .ForMember(dest => dest.QuestionExternalCode, opt => opt.MapFrom(src => src.QuestionExternalCode))
                .ForMember(dest => dest.TriviaQuestionCode, opt => opt.MapFrom(src => src.TriviaQuestionCode))
                .ForMember(dest => dest.CreateUser, opt => opt.MapFrom(src => src.CreateUser))
                .ForMember(dest => dest.TriviaJson, opt => opt.MapFrom(src => src.TriviaJson))
                 .ForMember(dest => dest.CreateTs, opt => opt.MapFrom(src => DateTime.UtcNow)) // Assuming CreateTs is set to the current time
                 .ForMember(dest => dest.UpdateTs, opt => opt.MapFrom(src => DateTime.UtcNow)) // Assuming UpdateTs is also set to the current time
            .ForMember(dest => dest.DeleteNbr, opt => opt.MapFrom(src => 0)) // Default value for DeleteNbr

                .ReverseMap();
        }
    }
}

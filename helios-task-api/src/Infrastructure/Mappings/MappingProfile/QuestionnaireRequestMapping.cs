using AutoMapper;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile
{
    public class QuestionnaireRequestMapping : Profile
    {
        public QuestionnaireRequestMapping()
        {
            CreateMap<Questionnaire, QuestionnaireModel>().ReverseMap();

            CreateMap<QuestionnaireQuestionRequestDto, QuestionnaireQuestionDto>().ReverseMap();
            CreateMap<QuestionnaireQuestionData, QuestionnaireQuestionDto>().ReverseMap();
            CreateMap<QuestionnaireQuestionRequestDto, QuestionnaireQuestionModel>()
                .ForMember(dest => dest.QuestionExternalCode, opt => opt.MapFrom(src => src.QuestionExternalCode))
                .ForMember(dest => dest.QuestionnaireQuestionCode, opt => opt.MapFrom(src => src.QuestionnaireQuestionCode))
                .ForMember(dest => dest.CreateUser, opt => opt.MapFrom(src => src.CreateUser))
                .ForMember(dest => dest.QuestionnaireJson, opt => opt.MapFrom(src => src.QuestionnaireJson))
                .ForMember(dest => dest.CreateTs, opt => opt.MapFrom(src => DateTime.UtcNow)) // Assuming CreateTs is set to the current time
                .ForMember(dest => dest.UpdateTs, opt => opt.MapFrom(src => DateTime.UtcNow)) // Assuming UpdateTs is also set to the current time
                .ForMember(dest => dest.DeleteNbr, opt => opt.MapFrom(src => 0)) // Default value for DeleteNbr
                .ReverseMap();
        }
    }
}

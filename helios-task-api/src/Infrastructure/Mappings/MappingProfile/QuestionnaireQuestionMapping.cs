using AutoMapper;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile
{
    public class QuestionnaireQuestionMapping : Profile
    {
        public QuestionnaireQuestionMapping()
        {
            CreateMap<QuestionnaireQuestionDto, QuestionnaireQuestionModel>().ReverseMap();
            CreateMap<QuestionnaireQuestionData, QuestionnaireQuestionModel>().ReverseMap();
        }
    }
}

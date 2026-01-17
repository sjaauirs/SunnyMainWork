using AutoMapper;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile
{
    public class QuestionnaireMapping : Profile
    {
        public QuestionnaireMapping()
        {
            CreateMap<QuestionnaireDto, QuestionnaireModel>().ReverseMap();
            CreateMap<QuestionnaireDataDto, QuestionnaireModel>().ReverseMap();
            CreateMap<QuestionnaireDto, Questionnaire>().ReverseMap();
        }
    }
}

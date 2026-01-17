using AutoMapper;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile
{
    public class QuestionnaireQuestionGroupMapping : Profile
    {
        public QuestionnaireQuestionGroupMapping()
        {
            CreateMap<QuestionnaireQuestionGroupDto, QuestionnaireQuestionGroupModel>().ReverseMap();
            CreateMap<QuestionnaireQuestionGroupPostRequestDto, QuestionnaireQuestionGroupModel>().ReverseMap();
            CreateMap<QuestionnaireQuestionGroupPostRequestDto, QuestionnaireQuestionGroupDto>().ReverseMap();
        }
    }
}

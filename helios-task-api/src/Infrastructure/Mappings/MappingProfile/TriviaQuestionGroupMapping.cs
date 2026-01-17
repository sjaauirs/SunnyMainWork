using AutoMapper;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile
{
    public class TriviaQuestionGroupMapping : Profile
    {
        public TriviaQuestionGroupMapping()
        {
            CreateMap<TriviaQuestionGroupDto, TriviaQuestionGroupModel>().ReverseMap();
            CreateMap<TriviaQuestionGroupPostRequestDto, TriviaQuestionGroupModel>().ReverseMap();
            CreateMap<TriviaQuestionGroupPostRequestDto, TriviaQuestionGroupDto>().ReverseMap();

        }
    }
}

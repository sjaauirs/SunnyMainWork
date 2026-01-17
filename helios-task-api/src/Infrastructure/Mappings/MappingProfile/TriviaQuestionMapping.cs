using AutoMapper;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile
{
    public class TriviaQuestionMapping : Profile
    {
        public TriviaQuestionMapping()
        {
            CreateMap<TriviaQuestionDto, TriviaQuestionModel>().ReverseMap();
            CreateMap<TriviaQuestionData, TriviaQuestionModel>().ReverseMap();

        }
    }
}

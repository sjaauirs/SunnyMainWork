using AutoMapper;
using SunnyRewards.Helios.Task.Core.Domain.Dtos;
using SunnyRewards.Helios.Task.Core.Domain.Models;

namespace SunnyRewards.Helios.Task.Infrastructure.Mappings.MappingProfile
{
    public class TriviaMapping : Profile
    {
        public TriviaMapping()
        {
            CreateMap<TriviaDto, TriviaModel>().ReverseMap();
            CreateMap<TriviaDataDto, TriviaModel>().ReverseMap();
            CreateMap<TriviaDto, Trivia>().ReverseMap();
        }
    }
}

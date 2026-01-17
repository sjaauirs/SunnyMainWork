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
    public class SubTaskMapping : Profile
    {
        public SubTaskMapping()
        {
            CreateMap<SubTaskDto, SubTaskModel>().ReverseMap();
            CreateMap<PostSubTaskDto, SubTaskModel>().ReverseMap();
            CreateMap<SubTaskUpdateRequestDto, SubTaskModel>().ReverseMap();
            CreateMap<SubTaskUpdateRequestDto, SubTaskDto>().ReverseMap();
        }
    }
}

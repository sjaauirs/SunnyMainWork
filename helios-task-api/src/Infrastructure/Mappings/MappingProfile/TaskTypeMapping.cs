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
    public class TaskTypeMapping : Profile
    {
        public TaskTypeMapping()
        {
            CreateMap<TaskTypeDto, TaskTypeModel>().ReverseMap();
        }
    }
}
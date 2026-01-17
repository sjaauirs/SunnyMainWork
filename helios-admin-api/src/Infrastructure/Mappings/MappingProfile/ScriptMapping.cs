using AutoMapper;
using SunnyRewards.Helios.Admin.Core.Domain.Dtos;
using SunnyRewards.Helios.Admin.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Admin.Infrastructure.Mappings.MappingProfile
{
    public class ScriptMapping : Profile
    {
        public ScriptMapping()
        {
            CreateMap<ScriptDto, ScriptModel>().ReverseMap();
        }
    }
}

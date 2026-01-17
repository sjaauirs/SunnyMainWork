using NHibernate.Linq.ResultOperators;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static NHibernate.Engine.Query.CallableParser;

namespace SunnyRewards.Helios.User.UnitTest.Fixtures.MockDto
{
    public class PersonMockDto : PersonModel
    {
        public PersonMockDto()
        {

            PersonId = 1;
            PersonCode = "per-91532506c783dd4601e1d27704";
            FirstName = "FirstName";
            LastName = "LastName";
            LanguageCode = "en-US-IND";
            MemberSince = DateTime.UtcNow;
            Email = "mock@example.com";
            City = "chd";
            Country = "india";
            YearOfBirth = 1999;
            PostalCode = "0711122";
            PhoneNumber = "8784738747";
            Region = "Region";
            DOB = DateTime.UtcNow;
            Gender = "Male";
            
        }
        public static List<PersonModel> personData()
        {
            return new List<PersonModel>()
            {
                new PersonMockDto()
            };
        }


    }
}

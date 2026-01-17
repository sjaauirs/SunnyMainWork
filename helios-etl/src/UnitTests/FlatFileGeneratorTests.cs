using SunnyRewards.Helios.ETL.Common.Domain.Config;
using SunnyRewards.Helios.ETL.Common.Domain.Enum;
using SunnyRewards.Helios.ETL.Common.Helpers;
using SunnyRewards.Helios.ETL.Common.Helpers.Interfaces;

namespace SunnyRewards.Helios.ETL.UnitTests
{
    public class FlatFileGeneratorTests
    {
        public class Person
        {
            public string? Name { get; set; }
            public int Age { get; set; }
            public string? Email { get; set; }
        }

        [Fact]
        public void GenerateFlatFile()
        {
            IFlatFileGenerator generator = new FlatFileGenerator();

            var person = new Person
            {
                Name = "John Doe",
                Age = 30,
                Email = "johndoe@example.com"
            };

            var fieldConfigurations = new Dictionary<string, FieldConfiguration>
            {
                { "Name", new FieldConfiguration(50, Justification.Left) },
                { "Age", new FieldConfiguration(3, Justification.Right, '0') },
                { "Email", new FieldConfiguration(50, Justification.Right, '=') }
            };

            string flatFileText = generator.GenerateFlatFileRecord(person, fieldConfigurations);
            Assert.Equal("John Doe                                          030===============================johndoe@example.com", flatFileText);
            Console.WriteLine(flatFileText);
        }
    }
}
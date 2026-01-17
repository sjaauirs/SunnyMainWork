using SunnyRewards.Helios.ETL.Common.Domain.Config;

namespace SunnyRewards.Helios.ETL.Common.Helpers.Interfaces
{
    /// <summary>
    /// Defines the interface for a generic flat text file generator
    /// </summary>
    public interface IFlatFileGenerator
    {
        string GenerateFlatFileRecord<T>(T modelObject, Dictionary<string, FieldConfiguration> fieldConfigurations);
    }
}

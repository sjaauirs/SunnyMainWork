using SunnyRewards.Helios.ETL.Common.Domain.Config;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Common.Helpers.Interfaces
{
    /// <summary>
    /// Defines the interface for a generic flat text file reader
    /// </summary>
    public interface IFlatFileReader
    {
        T ReadFlatFileRecord<T>(T modelObject, string record, Dictionary<string, FieldConfiguration> fieldConfigurations);
        T ReadFlatFileRecord<T>(string record, char splitter);
        
    }
}

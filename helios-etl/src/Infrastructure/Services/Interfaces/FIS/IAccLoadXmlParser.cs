using SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS;

namespace SunnyRewards.Helios.ETL.Infrastructure.Services.Interfaces.FIS
{
    public interface IAccLoadXmlParser
    {
        /// <summary>
        /// Parses the given ACC Load XML content to extract required information
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        FISAccLoadDto Parse(string xml);
    }
}

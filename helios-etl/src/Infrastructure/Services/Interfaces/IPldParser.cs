using SunnyRewards.Helios.Etl.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Etl.Infrastructure.Services.Interfaces
{
    public interface IPldParser
    {
        PldRecordDto ParsePldLine(string pldLine);
    }
}

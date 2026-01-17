namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.FIS
{
    public class FISAccLoadDto
    {
        public string CompanyId { get; set; } = ""; // UAT: 1204185 - from tenant config level-1 clientId (parentId of level-2)

        public string ClientId { get; set; } = "";

        public string SubprogramId { get; set; } = "";

        public string PackageId { get; set; } = "";
    }
}

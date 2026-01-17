namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class ProductIngestionMessageDto
    {
        public string? MessageName { get; set; }
        public DateTime Timestamp { get; set; }
        public string? SendingApplication { get; set; }
        public int BatchId { get; set; }
        public string? TrackingId { get; set; }
        public Item? Item { get; set; }
        public bool DisableDbBackup { get; set; }
        public string? ProductSku { get; set; }
    }

    public class Item
    {
        public string? Company { get; set; }
        public int Product { get; set; }
        public string? UpcNumber { get; set; }
        public int UpcIndicator { get; set; }
        public string? Action { get; set; }
        public Data? Data { get; set; }
    }

    public class Data
    {
        public string? DeptName { get; set; }
        public string? Manufacturer { get; set; }
        public string? ProdName { get; set; }
        public string? ProdSize { get; set; }
        public string? UOM { get; set; }
        public string? PkgSize { get; set; }
    }

}

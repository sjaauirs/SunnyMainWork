namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class FileDownloadResult : IDisposable
    {
        public Stream? FileStream { get; set; }
        public string? FileName { get; set; }

        public void Dispose()
        {
            FileStream?.Dispose();
        }
    }
}

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.Json
{

    public class SftpConfig
    {
        public string? Host { get; set; }
        public int Port { get; set; } = 22; // Default SFTP port
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? PrivateKey { get; set; }
        public string? RemoteDirectory { get; set; }
        public string? PrivateKeyPassphrase { get; set; }
    }

}

namespace SunnyRewards.Helios.Admin.UnitTest.Fixtures.MockDtos
{
    public static class AppSettingsMock
    {
        public static readonly Dictionary<string, string?> AppSettings = new Dictionary<string, string?>
        {
            {"AWS:AWS_ACCESS_KEY_NAME", "access-key"},
            {"AWS:AWS_SECRET_KEY_NAME", "secret-key"},
            {"AWS:AWS_TMP_BUCKET_NAME", "tmp-bucket"},
            {"ExportTenantVersion", "1.0"}
        };
    }
}

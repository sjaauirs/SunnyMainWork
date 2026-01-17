namespace SunnyRewards.Helios.ETL.Infrastructure.Extensions
{
    /// <summary>
    /// 
    /// </summary>
    public static class CommonExtension
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="byteArray"></param>
        /// <returns></returns>
        public static Stream ToStream(this byte[] byteArray)
        {
            // Create a new MemoryStream and write the byte array to it.
            using MemoryStream stream = new(byteArray)
            {
                // Set the position to the beginning of the stream to make it ready for reading.
                Position = 0
            };

            return stream;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetFile(this List<string> fileNames, string fileName)
        {
            return fileNames.FirstOrDefault(x => x.ToLower().StartsWith(fileName.ToLower())) ?? "";
        }

        /// <summary>
        /// Returns all matching files with given prefix
        /// </summary>
        /// <param name="fileNames"></param>
        /// <param name="prefix"></param>
        /// <returns></returns>
        public static List<string> GetMatchingFiles(this List<string> fileNames, string prefix)
        {
            return fileNames.Select(x => x).Where(x => x.ToLower().StartsWith(prefix)).ToList();
        }
    }
}

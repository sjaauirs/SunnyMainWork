using SunnyRewards.Helios.ETL.Common.Domain.Enum;

namespace SunnyRewards.Helios.ETL.Common.Domain.Config
{
    public class FieldConfiguration
    {
        public int Width { get; set; }
        public Justification Justification { get; set; }
        public char PaddingChar { get; set; }

        public FieldConfiguration(int width, Justification justification,
            char paddingChar = ' ')
        {
            Width = width;
            Justification = justification;
            PaddingChar = paddingChar;
        }
    }
}

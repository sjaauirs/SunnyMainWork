using SunnyRewards.Helios.ETL.Common.Domain.Config;
using SunnyRewards.Helios.ETL.Common.Domain.Enum;
using SunnyRewards.Helios.ETL.Common.Helpers.Interfaces;
using System.Reflection;
using System.Text;

namespace SunnyRewards.Helios.ETL.Common.Helpers
{
    public class FlatFileGenerator : IFlatFileGenerator
    {
        public string GenerateFlatFileRecord<T>(T modelObject, Dictionary<string, FieldConfiguration> fieldConfigurations)
        {
            StringBuilder flatFileContent = new StringBuilder();
            Type type = typeof(T);

            var baseProps = type.BaseType == null ? new List<PropertyInfo>() : type.BaseType.GetProperties().ToList();
            var derivedProps = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).ToList();

            baseProps.AddRange(derivedProps);
            List<PropertyInfo> properties = baseProps;

            foreach (PropertyInfo property in properties)
            {
                string propertyName = property.Name;
                object? value = property.GetValue(modelObject, null) ?? "";

                if (fieldConfigurations.TryGetValue(propertyName, out FieldConfiguration? config))
                {
                    string formattedValue = FormatValue(value.ToString(), config);
                    flatFileContent.Append($"{formattedValue}");
                }
                else
                {
                    flatFileContent.Append($"{value}");
                }
            }

            return flatFileContent.ToString();
        }

        private string FormatValue(string? value, FieldConfiguration config)
        {
            if (value == null)
                value = "";

            if (value.Length > config.Width)
            {
                return value.Substring(0, config.Width);
            }

            return config.Justification == Justification.Right ?
                value.PadLeft(config.Width, config.PaddingChar) :
                value.PadRight(config.Width, config.PaddingChar);
        }
    }
}
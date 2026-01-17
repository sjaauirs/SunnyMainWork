using Microsoft.AspNetCore.Http;
using SunnyRewards.Helios.ETL.Common.Domain.Config;
using SunnyRewards.Helios.ETL.Common.Helpers.Interfaces;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace SunnyRewards.Helios.ETL.Common.Helpers
{
    public class FlatFileReader : IFlatFileReader
    {
        public T ReadFlatFileRecord<T>(T modelObject, string record, Dictionary<string, FieldConfiguration> fieldConfigurations)
        {
            Type type = typeof(T);
            var baseProps = type.BaseType == null ? new List<PropertyInfo>() : type.BaseType.GetProperties().ToList();
            var derivedProps = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).ToList();

            baseProps.AddRange(derivedProps);
            List<PropertyInfo> properties = baseProps;

            int index = 0;
            foreach (var property in properties)
            {
                string propertyName = property.Name;
                object? defaultValue = property.PropertyType.IsValueType ? Activator.CreateInstance(property.PropertyType) : null;

                if (fieldConfigurations.TryGetValue(propertyName, out FieldConfiguration? config))
                {
                    string formattedValue = record.Substring(index, config.Width)?.Trim() ?? "";

                    var valueWithRightType = string.IsNullOrEmpty(formattedValue)
                        ? defaultValue
                        : Convert.ChangeType(formattedValue, property.PropertyType);
                    if (property.CanWrite)
                    {
                        property.SetValue(modelObject, valueWithRightType);
                    }
                    index = index + config.Width;
                }
                else
                {
                    property.SetValue(modelObject, defaultValue);
                }
            }

            return modelObject;
        }
        public T ReadFlatFileRecord<T>(string record, char splitter)
        {
            var properties = typeof(T).GetProperties();

            T modelObject = (T)Activator.CreateInstance(typeof(T));
            var stringData = record.Trim().Split(splitter);

            for (int i = 0; i < Math.Min(stringData.Length, properties.Length); i++)
            {
                var property = properties[i];               
                var targetType = property.PropertyType;
                if (Nullable.GetUnderlyingType(property.PropertyType)!= null)
                {
                    targetType = Nullable.GetUnderlyingType(property.PropertyType);
                }
                if (stringData[i].Trim() == "Null" || string.IsNullOrEmpty(stringData[i]))
                {
                    property.SetValue(modelObject, null);
                    continue;
                }

                if (targetType == typeof(DateTime))
                {
                    string[] formats = ["MMddyyyy HH:mm:ss", "MMddyyyy"];
                    DateTime.TryParseExact(stringData[i], formats, CultureInfo.InvariantCulture,DateTimeStyles.None, out DateTime dateTimeValue);
                    property.SetValue(modelObject, dateTimeValue);
                }
                else
                {
                    object value = Convert.ChangeType(stringData[i], targetType);
                    property.SetValue(modelObject, value);
                }

            }
            return modelObject;
        }
    }
}

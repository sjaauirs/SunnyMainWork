using System.ComponentModel.DataAnnotations;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos.Validation
{
    public class RequiredIfAttribute : ValidationAttribute
    {
        private readonly string _dependentProperty;
        private readonly object _targetValue;

        public RequiredIfAttribute(string dependentProperty, object targetValue)
        {
            _dependentProperty = dependentProperty;
            _targetValue = targetValue;
        }

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var property = validationContext.ObjectType.GetProperty(_dependentProperty);
            if (property == null)
            {
                return ValidationResult.Success;
            }

            var dependentValue = property.GetValue(validationContext.ObjectInstance, null);

            if (dependentValue != null && dependentValue.Equals(_targetValue))
            {
                if (value == null || string.IsNullOrEmpty(value.ToString()))
                {
                    return new ValidationResult(ErrorMessage ?? $"{validationContext.MemberName} is required.");
                }
            }

            return ValidationResult.Success;
        }
    }
}

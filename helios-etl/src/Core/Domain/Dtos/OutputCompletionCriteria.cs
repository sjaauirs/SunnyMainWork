using Newtonsoft.Json;
using System.Collections.Generic;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class OutputCompletionCriteria
    {
        [JsonProperty("completionCriteriaType", NullValueHandling = NullValueHandling.Ignore)]
        public string? CompletionCriteriaType { get; set; }

        [JsonProperty("selfReportType", NullValueHandling = NullValueHandling.Ignore)]
        public string? SelfReportType { get; set; }

        [JsonProperty("completionPeriodType", NullValueHandling = NullValueHandling.Ignore)]
        public string? CompletionPeriodType { get; set; }

        [JsonProperty("imageCriteria", NullValueHandling = NullValueHandling.Ignore)]
        public ImageCriteriaOutput? ImageCriteria { get; set; }

        [JsonProperty("healthCriteria", NullValueHandling = NullValueHandling.Ignore)]
        public HealthCriteriaOutputBase? HealthCriteria { get; set; }

        [JsonProperty("disableTriviaSplashScreen", NullValueHandling = NullValueHandling.Ignore)]
        public bool? DisableTriviaSplashScreen { get; set; }

        
    }
    public class OutputCompletionCriteriaCustom
    {
        [JsonProperty("completionCriteriaType", NullValueHandling = NullValueHandling.Ignore)]
        public string? CompletionCriteriaType { get; set; }

        [JsonProperty("selfReportType", NullValueHandling = NullValueHandling.Ignore)]
        public string? SelfReportType { get; set; }

        [JsonProperty("completionPeriodType", NullValueHandling = NullValueHandling.Ignore)]
        public string? CompletionPeriodType { get; set; }

        [JsonProperty("healthCriteria", NullValueHandling = NullValueHandling.Ignore)]
        public CustomHealthCriteriaOutput? HealthCriteriaCustom { get; set; }
    }
    // ============================================================
    // 🖼️ IMAGE CRITERIA
    // ============================================================
    public class ImageCriteriaOutput
    {
        [JsonProperty("icon", NullValueHandling = NullValueHandling.Ignore)]
        public Icon? Icon { get; set; }

        [JsonProperty("unitLabel", NullValueHandling = NullValueHandling.Ignore)]
        public LocalizedText? UnitLabel { get; set; }
        
        [JsonProperty("unitType", NullValueHandling = NullValueHandling.Ignore)]
        public string? UnitType { get; set; }

        [JsonProperty("buttonLabel", NullValueHandling = NullValueHandling.Ignore)]
        public LocalizedText? ButtonLabel { get; set; }

        [JsonProperty("imageCriteriaText", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, List<ImageDescriptionItem>>? ImageCriteriaText { get; set; }


        [JsonProperty("requiredImageCount", NullValueHandling = NullValueHandling.Ignore)]
        public int? RequiredImageCount { get; set; }

        [JsonProperty("imageCriteriaTextAlignment", NullValueHandling = NullValueHandling.Ignore)]
        public string? ImageCriteriaTextAlignment { get; set; }
    }

    public class Icon
    {
        [JsonProperty("url", NullValueHandling = NullValueHandling.Ignore)]
        public string? Url { get; set; }
    }

    // ============================================================
    // ❤️‍🩹 HEALTH CRITERIA (Base + Derived)
    // ============================================================
    public abstract class HealthCriteriaOutputBase
    {
        [JsonProperty("buttonLabel", NullValueHandling = NullValueHandling.Ignore)]
        public LocalizedText? ButtonLabel { get; set; }

        [JsonProperty("unitLabel", NullValueHandling = NullValueHandling.Ignore)]
        public LocalizedText? UnitLabel { get; set; }
        [JsonProperty("unitType", NullValueHandling = NullValueHandling.Ignore)]
        public string? UnitType { get; set; }
        [JsonProperty("inputLable", NullValueHandling = NullValueHandling.Ignore)]
        public LocalizedText? InputLable { get; set; }
        [JsonProperty("inputplaceholder", NullValueHandling = NullValueHandling.Ignore)]
        public LocalizedText? InputPlaceholder { get; set; }

        [JsonProperty("healthTaskType", NullValueHandling = NullValueHandling.Ignore)]
        public string? HealthTaskType { get; set; }
    }

    public class HealthCriteriaStepsOutput : HealthCriteriaOutputBase
    {
        [JsonProperty("requiredSteps", NullValueHandling = NullValueHandling.Ignore)]
        public int? RequiredSteps { get; set; }
    }

    public class HealthCriteriaSleepOutput : HealthCriteriaOutputBase
    {
        [JsonProperty("requiredSleep", NullValueHandling = NullValueHandling.Ignore)]
        public RequiredSleep? RequiredSleep { get; set; }
    }

    public class HealthCriteriaOtherOutput : HealthCriteriaOutputBase
    {
        [JsonProperty("requiredUnits", NullValueHandling = NullValueHandling.Ignore)]
        public int? RequiredUnits { get; set; }
    }

    // ============================================================
    // ⚙️ CUSTOM COMPLETION CRITERIA
    // ============================================================
   

    public class CustomHealthCriteriaOutput
    {
        [JsonProperty("unitType", NullValueHandling = NullValueHandling.Ignore)]
        public string? UnitType { get; set; }

        [JsonProperty("unitLabel", NullValueHandling = NullValueHandling.Ignore)]
        public LocalizedText? UnitLabel { get; set; }

        [JsonProperty("buttonLabel", NullValueHandling = NullValueHandling.Ignore)]
        public LocalizedText? ButtonLabel { get; set; }

        [JsonProperty("requiredUnits", NullValueHandling = NullValueHandling.Ignore)]
        public int? RequiredUnits { get; set; }

        [JsonProperty("healthTaskType", NullValueHandling = NullValueHandling.Ignore)]
        public string? HealthTaskType { get; set; }

        [JsonProperty("skipDisclaimer", NullValueHandling = NullValueHandling.Ignore)]
        public bool SkipDisclaimer { get; set; }

        [JsonProperty("isDialerRequired", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsDialerRequired { get; set; }

        [JsonProperty("isDisclaimerAutoChecked", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsDisclaimerAutoChecked { get; set; }

        [JsonProperty("uiComponents", NullValueHandling = NullValueHandling.Ignore)]
        public List<UIComponent>? UIComponents { get; set; }
    }

    // ============================================================
    // 🧩 UI COMPONENT
    // ============================================================
    public class UIComponent
    {
        [JsonProperty("componentType", NullValueHandling = NullValueHandling.Ignore)]
        public string? ComponentType { get; set; }

        [JsonProperty("reportTypeLabel", NullValueHandling = NullValueHandling.Ignore)]
        public LocalizedText? ReportTypeLabel { get; set; }

        [JsonProperty("placeholder", NullValueHandling = NullValueHandling.Ignore)]
        public LocalizedText? Placeholder { get; set; }

        [JsonProperty("multiSelect", NullValueHandling = NullValueHandling.Ignore)]
        public bool? MultiSelect { get; set; }
        [JsonProperty("selfReportType", NullValueHandling = NullValueHandling.Ignore)]
        public string? SelfReportType { get; set; }

        [JsonProperty("isRequiredField", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsRequiredField { get; set; }

        [JsonProperty("options", NullValueHandling = NullValueHandling.Ignore)]
        public List<Option>? Options { get; set; }
    }

    // ============================================================
    // 🧾 OPTION & RICH TEXT DISPLAY
    // ============================================================
    public class Option
    {
        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string? Value { get; set; }

        [JsonProperty("label", NullValueHandling = NullValueHandling.Ignore)]
        public LocalizedText? Label { get; set; }
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string? Type { get; set; }
        [JsonProperty("modalImageUrl", NullValueHandling = NullValueHandling.Ignore)]
        public string? ModalImageUrl { get; set; }

        [JsonProperty("onSelectionDisplay", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, List<RichTextElement>>? OnSelectionDisplay { get; set; }
    }

    public class RichTextElement
    {
        [JsonProperty("type", NullValueHandling = NullValueHandling.Ignore)]
        public string? Type { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string? Value { get; set; }

      
    }

   
}

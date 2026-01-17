using Newtonsoft.Json;
using System.Collections.Generic;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    // 🔹 Base Class
    public abstract class BaseCompletionCriteriaInput
    {
        public string? CompletionCriteriaType { get; set; }
    }

    // ======================================================
    // 1️⃣ IMAGE Completion Criteria
    // ======================================================
    public class ImageCompletionCriteriaInput : BaseCompletionCriteriaInput
    {
        [JsonProperty("imageCriteria")]
        public ImageCriteriaInput? ImageCriteria { get; set; }
    }

    public class ImageCriteriaInput
    {
        [JsonProperty("imageDescription")]
        public Dictionary<string, List<ImageDescriptionItem>>? ImageDescription { get; set; }

        [JsonProperty("requiredImageCount")]
        public int? RequiredImageCount { get; set; }

        [JsonProperty("ofRequiredImageCountLabel")]
        public LocalizedText? OfRequiredImageCountLabel { get; set; }

        [JsonProperty("buttonLabel")]
        public LocalizedText? ButtonLabel { get; set; }

        [JsonProperty("imageCriteriaIconUrl")]
        public string? ImageCriteriaIconUrl { get; set; }

        [JsonProperty("imageCriteriaTextAlignment")]
        public string? ImageCriteriaTextAlignment { get; set; }
    }
    public class ImageDescriptionItem
    {
        [JsonProperty("type")]

        public string? Type { get; set; }
        [JsonProperty("data")]
        public ImageDescriptionData? Data { get; set; }
    }
    public class ImageDescriptionData
    {
        [JsonProperty("text")]

        public string? Text { get; set; }
    }
    // ======================================================
    // 2️⃣ HEALTH Completion Criteria (Steps, Sleep, Other)
    // ======================================================
    public class HealthCompletionCriteriaInput : BaseCompletionCriteriaInput
    {
        [JsonProperty("healthCriteria")]
        public HealthCriteriaInput? HealthCriteria { get; set; }
    }

    public class HealthCriteriaInput
    {
        [JsonProperty("buttonLabel")]
        public LocalizedText? ButtonLabel { get; set; }

        [JsonProperty("inputPlaceholder")]
        public LocalizedText? InputPlaceholder { get; set; }

        [JsonProperty("inputLabel")]
        public LocalizedText? InputLabel { get; set; }

        [JsonProperty("ofRequiredStepCountLabel")]
        public LocalizedText? OfRequiredStepCountLabel { get; set; }

        [JsonProperty("ofRequiredSleepCountLabel")]
        public LocalizedText? OfRequiredSleepCountLabel { get; set; }

        [JsonProperty("ofRequiredCountLabel")]
        public LocalizedText? OfRequiredCountLabel { get; set; }

        [JsonProperty("ofRequiredAddSubtractInteraction")]
        public LocalizedText? OfRequiredAddSubtractInteraction { get; set; }

        [JsonProperty("requiredSteps")]
        public int? RequiredSteps { get; set; }

        [JsonProperty("requiredUnits")]
        public int? RequiredUnits { get; set; }

        [JsonProperty("requiredSleep")]
        public RequiredSleep? RequiredSleep { get; set; }
    }

    // ======================================================
    // 3️⃣ CUSTOM Complex Health Criteria (Dropdown + Mixed)
    // ======================================================
    public class CustomCompletionCriteriaInput : BaseCompletionCriteriaInput
    {
        [JsonProperty("healthCriteria")]
        public CustomHealthCriteriaInput? HealthCriteria { get; set; }
    }

    public class CustomHealthCriteriaInput
    {
        [JsonProperty("completionComponents")]
        public List<string>? CompletionComponents { get; set; }

        [JsonProperty("completionComponentLabels")]
        public Dictionary<string, List<string>>? CompletionComponentLabels { get; set; }

        [JsonProperty("completionComponentPlaceholders")]
        public Dictionary<string, List<string>>? CompletionComponentPlaceholders { get; set; }

        [JsonProperty("completionButtonLabel")]
        public LocalizedText? CompletionButtonLabel { get; set; }

        [JsonProperty("ofRequiredAddSubtractInteraction")]
        public LocalizedText? OfRequiredAddSubtractInteraction { get; set; }

        [JsonProperty("requiredTaskCompletionCount")]
        public int? RequiredTaskCompletionCount { get; set; }

        [JsonProperty("requiredCompletionComponent")]
        public string? RequiredCompletionComponent { get; set; }

        [JsonProperty("disclaimerRequired")]
        public string? DisclaimerRequired { get; set; }

        [JsonProperty("isDisclaimerAutochecked")]
        public string? IsDisclaimerAutochecked { get; set; }

        [JsonProperty("isDialerRequire")]
        public string? IsDialerRequire { get; set; }

        [JsonProperty("completionComponentAlignment")]
        public string? CompletionComponentAlignment { get; set; }

        [JsonProperty("dropdownConfigurations")]
        public List<DropdownConfigurationInput>? DropdownConfigurations { get; set; }
    }

    // ======================================================
    // 4️⃣ Dropdown Configuration & Options
    // ======================================================
    public class DropdownConfigurationInput
    {
        [JsonProperty("configurationFor")]
        public string? ConfigurationFor { get; set; }

        [JsonProperty("selectionType")]
        public string? SelectionType { get; set; }

        [JsonProperty("options")]
        public Dictionary<string, List<string>>? Options { get; set; }

        [JsonProperty("optionsSelectionCriteria")]
        public Dictionary<string, OptionSelectionCriteria>? OptionsSelectionCriteria { get; set; }
    }

    public class OptionSelectionCriteria
    {
        [JsonProperty("modalPopup")]
        public ModalPopup? ModalPopup { get; set; }

        [JsonProperty("optionDescription")]
        public OptionDescription? OptionDescription { get; set; }
    }

    public class ModalPopup
    {
        [JsonProperty("modalTextEnUsHeader")]
        public string? ModalTextEnUsHeader { get; set; }

        [JsonProperty("modalTextEsHeader")]
        public string? ModalTextEsHeader { get; set; }

        [JsonProperty("modalTextEnUsDescription")]
        public string? ModalTextEnUsDescription { get; set; }

        [JsonProperty("modalTextEsDescription")]
        public string? ModalTextEsDescription { get; set; }

        [JsonProperty("modalImageUrl")]
        public string? ModalImageUrl { get; set; }
    }

    public class OptionDescription
    {
        [JsonProperty("descriptionTextHeaderEnUs")]
        public string? DescriptionTextHeaderEnUs { get; set; }

        [JsonProperty("descriptionTextHeaderEs")]
        public string? DescriptionTextHeaderEs { get; set; }

        [JsonProperty("descriptionTextBodyEnUs")]
        public string? DescriptionTextBodyEnUs { get; set; }

        [JsonProperty("descriptionTextBodyEs")]
        public string? DescriptionTextBodyEs { get; set; }
    }

    // ======================================================
    // 5️⃣ TRIVIA Criteria
    // ======================================================
    public class TriviaCompletionCriteriaInput : BaseCompletionCriteriaInput
    {
        [JsonProperty("disableTriviaSplashScreen")]
        public bool? DisableTriviaSplashScreen { get; set; }
    }

    // ======================================================
    // Common Inner Types
    // ======================================================
    public class LocalizedText : Dictionary<string, string> { }

    public class ContentBlock
    {
        [JsonProperty("type")]
        public string? Type { get; set; }

        [JsonProperty("data")]
        public TextData? Data { get; set; }
    }

    public class TextData
    {
        [JsonProperty("text")]
        public string? Text { get; set; }
    }

    public class RequiredSleep
    {
        [JsonProperty("minSleepDuration")]
        public int? MinSleepDuration { get; set; }

        [JsonProperty("numDaysAtOrAboveMinDuration")]
        public int? NumDaysAtOrAboveMinDuration { get; set; }
    }
    public class TaskCompletionCriteriaWrapper
    {
        [JsonProperty("healthCriteria")]
        public object? HealthCriteria { get; set; }
    }

}

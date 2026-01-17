using CsvHelper.Configuration.Attributes;
namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class FisRecordDto
    {
        [Name("Action")]
        public string? Action { get; set; }

        [Name("UPC/PLU Value")]
        public string? UpcOrPluValue { get; set; }

        [Name("UPC/PLU Data Length")]
        public int? UpcOrPluDataLength { get; set; }

        [Name("UPC/PLU Indicator")]
        public string? UpcOrPluIndicator { get; set; }

        [Name("Manufacturer")]
        public string? Manufacturer { get; set; }

        [Name("Brand")]
        public string? Brand { get; set; }

        [Name("Product Name (150 Char)")]
        public string? ProductName { get; set; }

        [Name("Product Short Name (25 char)")]
        public string? ProductShortName { get; set; }

        [Name("Product Size")]
        public string? ProductSize { get; set; }

        [Name("Unit of Measure")]
        public string? UnitOfMeasure { get; set; }

        [Name("Package Size")]
        public string? PkgSize { get; set; }

        [Name("Generic Categorization or Department Information")]
        public string? DeptName { get; set; }

        [Name("Product Image")]
        public string? ProductImage { get; set; }

        [Name("Nutritional Information Image")]
        public string? NutritionalInformationImage { get; set; }

        [Name("Ingredients Image")]
        public string? IngredientsImage { get; set; }

        [Name("Drug Facts Image")]
        public string? DrugFactsImage { get; set; }

        [Name("Additional Images")]
        public string? AdditionalImages { get; set; }

        [Name("Company")]
        public string? Company { get; set; }

        [Name("Product SKU")]
        public string? ProductSKU { get; set; }

    }
}

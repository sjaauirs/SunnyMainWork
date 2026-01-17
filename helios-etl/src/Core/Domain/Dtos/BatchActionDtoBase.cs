namespace SunnyRewards.Helios.Etl.Core.Domain.Dtos
{
    public abstract class BatchActionDtoBase
    {
        public string BatchAction { get; set; } = string.Empty;
    }
    public class ETLlocationDto
    {
        public ETLlocationDto(string storageName , string folderName , string fileName)
        {
            StorageName = storageName;
            FolderName = folderName;
            FileName = fileName;
        }
        public string StorageName { get; set; }
        public string FolderName { get; set; } 
        public string FileName { get; set; } 
    }

    public class GenerateActionDto : BatchActionDtoBase
    {
        public ETLlocationDto Location { get; set; } 
    }
    public class EncryptActionDto : BatchActionDtoBase
    {
        public ETLlocationDto SrcLocation { get; set; } 
        public ETLlocationDto DstLocation { get; set; } 
    }

    public class CopyActionDto : BatchActionDtoBase
    {
        public ETLlocationDto SrcLocation { get; set; } 
        public ETLlocationDto DstLocation { get; set; } 
    }

    public class DeleteActionDto : BatchActionDtoBase
    {
        public ETLlocationDto Location { get; set; } 
    }
}

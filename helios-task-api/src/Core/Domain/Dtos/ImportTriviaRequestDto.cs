using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.Task.Core.Domain.Dtos
{
    public class ImportTriviaRequestDto
    {
        public ImportTriviaDetailDto? TriviaDetailDto { get; set; }
        public string? TenantCode { get; set; }
    }
    public class ImportTriviaDetailDto
    {
        public List<ExportTriviaDto>? Trivia { get; set; }
        public List<TriviaQuestionGroupDto>? TriviaQuestionGroup { get; set; }
        public List<TriviaQuestionDto>? TriviaQuestion { get; set; }
    }
    public class ImportTriviaQuestionMappingDto
    {
        public List<ImportTriviaQuestionDto>? TriviaCodeMapping { get; set; }
        public List<ImportTriviaQuestionDto>? TriviaQuestionMapping { get; set; }
    }
    public class ImportTriviaQuestionDto
    {
        public long? Id { get; set; }
        public string? Code { get; set; }
    }

}

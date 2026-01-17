using SunnyRewards.Helios.Etl.Core.Domain.Dtos;
using SunnyRewards.Helios.Etl.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Etl.Infrastructure.Services
{
    public class PldParser : IPldParser
    {
        private readonly IPldFieldInfoProvider _pldFieldInfoProvider;

        public PldParser(IPldFieldInfoProvider pldFieldInfoProvider)
        {
            _pldFieldInfoProvider = pldFieldInfoProvider;
        }

        public PldRecordDto ParsePldLine(string pldLine)
        {
            PldRecordDto rec = new PldRecordDto();

            var fieldInfo = _pldFieldInfoProvider.GetFieldInfo();

            int currOffset = 0;
            foreach (var field in fieldInfo)
            {
                try
                {
                    if (field.FieldDescription != null && field.FieldIdentifier != null)
                    {
                        string fieldValue = pldLine.Substring(currOffset, field.FieldLength);
                        rec.PldFieldData[field.FieldIdentifier] = fieldValue.Trim();
                        currOffset += field.FieldLength;
                    }
                }
                catch (Exception)
                {
                    // TODO: log error
                    break;
                }
            }

            return rec;
        }
    }
}

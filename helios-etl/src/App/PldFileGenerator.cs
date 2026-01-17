using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SunnyRewards.Helios.Etl.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Etl.App
{
    public class PldFileGenerator
    {
        public void Generate(IHost? host, string memNbrFilePath, string outputPldFilePath)
        {
            var rand = new Random();

            var pldFieldInfoProvider = host?.Services.GetRequiredService<IPldFieldInfoProvider>();
            var fieldInfo = pldFieldInfoProvider?.GetFieldInfo();

            if (fieldInfo == null)
            {
                // TODO: log error
                return;
            }

            using StreamReader streamReader = new(memNbrFilePath);
            using StreamWriter streamWriter = new(outputPldFilePath);
            string? line;
            while ((line = streamReader.ReadLine()) != null)
            {
                line = line.Trim();

                int prevDenom = 0;
                string outputLine = "";

                foreach (var field in fieldInfo)
                {
                    if (field.FieldIdentifier == "mbi_a_ben_s_med_ben_ide")
                    {
                        outputLine += $"{line,-11}";
                        continue;
                    }

                    var fieldDesc = field.FieldDescription?.ToLower();
                    if (fieldDesc != null)
                    {
                        if (fieldDesc.Contains("denominator"))
                        {
                            switch (field.FieldLength)
                            {
                                case 1:
                                    var val1 = rand.Next(0, 2);
                                    prevDenom = val1;
                                    outputLine += $"{val1,-1}";
                                    continue;

                                case 2:
                                    var val2 = rand.Next(0, 10);
                                    prevDenom = val2;
                                    outputLine += $"{val2,-2}";
                                    continue;
                            }
                        }
                        else if (fieldDesc.Contains("numerator"))
                        {
                            switch (field.FieldLength)
                            {
                                case 1:
                                    var val1 = rand.Next(0, prevDenom + 1);
                                    outputLine += $"{val1,-1}";
                                    continue;

                                case 2:
                                    var val2 = rand.Next(0, prevDenom + 1);
                                    outputLine += $"{val2,-2}";
                                    continue;
                            }
                        }
                    }

                    if (field.FieldLength == 3)
                    {
                        outputLine += $"{rand.Next(0, 100),-3}";
                        continue;
                    }
                    else if (field.FieldLength == 12)
                    {
                        outputLine += "0.0000000000";
                        continue;
                    }

                    outputLine += new string('X', field.FieldLength);
                }

                streamWriter.WriteLine(outputLine);
            }
        }
    }
}

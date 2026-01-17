using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.ETL.Core.Domain.Enums
{
    public enum CompletionCriteriaType
    {
        IMAGE,
        STEPS,
        SLEEP,
        OTHER,
        TRIVIA
    }
    public enum CompletionCriteriaComponentType
    {
        IMAGE_UPLOAD_BUTTON,
        INPUT,
        INTERACTIVE,
        CUSTOM
        
    }

}

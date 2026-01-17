using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.User.Core.Domain.enums
{
    public enum OnboardingState
    {
        NOT_STARTED,
        EMAIL_VERIFIED,
        DOB_VERIFIED,
        CARD_LAST_4_VERIFIED,
        PICK_A_PURSE_COMPLETED,
        COSTCO_ACTIONS_VISITED,
        AGREEMENT_VERIFIED,
        VERIFIED
    }
}

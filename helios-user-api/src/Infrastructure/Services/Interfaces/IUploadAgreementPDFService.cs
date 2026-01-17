using SunnyRewards.Helios.User.Core.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SunnyRewards.Helios.User.Infrastructure.Services.Interfaces
{
    public interface IUploadAgreementPDFService
    {
        Task<Dictionary<string, string>> UploadAgreementPDf(
              UpdateOnboardingStateDto verifyMemberDto,
              string tenantCode,
              string consumerCode);    }
}

using SunnyBenefits.Fis.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ICsaTransactionService
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="csaTransactionRequestDto"></param>
        /// <returns></returns>
        Task<CsaTransactionResponseDto> DisposeCsaTransaction(CsaTransactionRequestDto csaTransactionRequestDto);
    }
}

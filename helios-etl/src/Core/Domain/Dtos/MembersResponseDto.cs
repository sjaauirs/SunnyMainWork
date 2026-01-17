using SunnyRewards.Helios.ETL.Common.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;

namespace SunnyRewards.Helios.ETL.Core.Domain.Dtos
{
    public class MembersResponseDto : ExtendedErrorResponseDto
    {
        public List<ConsumerDataResponseDto> Consumers { get; set; } = new List<ConsumerDataResponseDto>();
        public List<ConsumerWalletDataResponseDto> ConsumerWallets { get; set; } = new List<ConsumerWalletDataResponseDto>();
    }
}

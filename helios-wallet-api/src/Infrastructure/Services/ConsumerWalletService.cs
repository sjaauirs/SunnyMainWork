using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Common.Core.Services;
using SunnyRewards.Helios.Wallet.Core.Domain.Dtos;
using SunnyRewards.Helios.Wallet.Core.Domain.Models;
using SunnyRewards.Helios.Wallet.Infrastructure.Repositories.Interfaces;
using SunnyRewards.Helios.Wallet.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Wallet.Infrastructure.Services
{
    public class ConsumerWalletService : BaseService, IConsumerWalletService
    {
        private readonly ILogger<ConsumerWalletService> _consumerWalletLogger;
        private readonly IConsumerWalletRepo _consumerWalletRepo;
        private readonly IWalletRepo _walletRepo;
        private readonly IWalletTypeRepo _walletTypeRepo;
        private readonly IMapper _mapper;
        private readonly NHibernate.ISession _session;
        const string className = nameof(ConsumerWalletService);
        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerWalletLogger"></param>
        /// <param name="consumerWalletRepo"></param>
        /// <param name="mapper"></param>
        public ConsumerWalletService(ILogger<ConsumerWalletService> consumerWalletLogger,
            IConsumerWalletRepo consumerWalletRepo, IMapper mapper, IWalletRepo walletRepo, IWalletTypeRepo walletTypeRepo
            , NHibernate.ISession session)
        {
            _consumerWalletLogger = consumerWalletLogger;
            _consumerWalletRepo = consumerWalletRepo;
            _mapper = mapper;
            _walletRepo = walletRepo;
            _walletTypeRepo = walletTypeRepo;
            _session = session;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerWalletRequestDto"></param>
        /// <returns></returns>
        public async Task<FindConsumerWalletResponseDto> GetConsumerWallet(FindConsumerWalletRequestDto consumerWalletRequestDto)
        {
            const string methodName = nameof(GetConsumerWallet);
            try
            {
                var consumerWalletDetails = await _consumerWalletRepo.GetConsumerWalletsWithDetails(consumerWalletRequestDto.ConsumerCode ?? string.Empty);
                if (consumerWalletDetails.Count <= 0)
                {
                    _consumerWalletLogger.LogError("{className}.{methodName}: Consumer Wallets Count less than zero. For Consumer Code:{consumerCode}, Error Code:{errorCode}", className, methodName, consumerWalletRequestDto.ConsumerCode, StatusCodes.Status404NotFound);
                    return new FindConsumerWalletResponseDto();
                }
                var consumerWallets = consumerWalletDetails.Select(x => x.ConsumerWallet).ToList(); 
                var response = _mapper.Map<List<ConsumerWalletDto>>(consumerWallets);
                var findConsumerResponseDto = new FindConsumerWalletResponseDto() { ConsumerWallets = response };

                _consumerWalletLogger.LogInformation("{className}.{methodName}: Retrieved ConsumerWallet Details Successfully for ConsumerCode : {ConsumerCode}", className, methodName, consumerWalletRequestDto.ConsumerCode);

                return findConsumerResponseDto;
            }
            catch (Exception ex)
            {
                _consumerWalletLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                return new FindConsumerWalletResponseDto() { ErrorMessage = ex.Message, ErrorDescription = ex.InnerException?.Message };

            }
        }

        public async Task<FindConsumerWalletResponseDto> GetConsumerWalletsByWalletType(FindConsumerWalletByWalletTypeRequestDto consumerWalletByWalletTypeRequestDto)
        {
            const string methodName = nameof(GetConsumerWalletsByWalletType);
            try
            {
                var consumerCode = consumerWalletByWalletTypeRequestDto.ConsumerCode;
                var walletTypeCode = consumerWalletByWalletTypeRequestDto.WalletTypeCode;

                var walletTypeData = await _walletTypeRepo.FindOneAsync(x => x.WalletTypeCode == walletTypeCode && x.DeleteNbr == 0);
                if (walletTypeData == null)
                {
                    _consumerWalletLogger.LogError("{className}.{methodName}: Wallet Type Data Not Found For Wallet Type Code:{walletType}, Error Code:{errorCode}", className, methodName, consumerWalletByWalletTypeRequestDto.WalletTypeCode, StatusCodes.Status404NotFound);
                    return new FindConsumerWalletResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "Wallet type not found"
                    };
                }


                var consumerWallet = await _consumerWalletRepo.GetConsumerWalletsByWalletType(consumerCode, walletTypeData.WalletTypeId);
                if (consumerWallet.Count <= 0)
                {
                    _consumerWalletLogger.LogError("{className}.{methodName}: Consumer Wallet Data Not Found For Consumer Code:{consumerCode}, Error Code:{errorCode}", className, methodName, consumerWalletByWalletTypeRequestDto.ConsumerCode, StatusCodes.Status404NotFound);
                    return new FindConsumerWalletResponseDto
                    {
                        ErrorCode = StatusCodes.Status404NotFound,
                        ErrorMessage = "Consumer wallet not found"
                    };
                }
                var response = _mapper.Map<List<ConsumerWalletDto>>(consumerWallet);
                var findConsumerResponseDto = new FindConsumerWalletResponseDto() { ConsumerWallets = response };

                _consumerWalletLogger.LogInformation("{className}.{methodName}: Retrieved ConsumerWallet Details Successfully for ConsumerCode : {ConsumerCode}", className, methodName, consumerCode);

                return findConsumerResponseDto;
            }
            catch (Exception ex)
            {
                _consumerWalletLogger.LogError(ex, "{className}.{methodName}: ERROR - msg : {msg}", className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="consumerWalletDataDto"></param>
        /// <returns></returns>
        public async Task<List<ConsumerWalletDataResponseDto>> PostConsumerWallets(IList<ConsumerWalletDataDto> consumerWalletDataDto)
        {
            const string methodName = nameof(PostConsumerWallets);
            try
            {
                List<ConsumerWalletDataResponseDto> responseDtos = new List<ConsumerWalletDataResponseDto>();
                foreach (var item in consumerWalletDataDto)
                {
                    var transaction = _session.BeginTransaction();
                    try
                    {
                        var walletModel = await _walletRepo.FindOneAsync(x => x.WalletId == item.walletDto.WalletId
                        && x.ActiveStartTs <= DateTime.UtcNow &&            // while retrieving consumer wallet, ensure it's active
                           x.ActiveEndTs >= DateTime.UtcNow
                        && x.DeleteNbr == 0);
                        if (walletModel == null)
                        {
                            item.walletDto.CreateUser = "SYSTEM";
                            item.walletDto.CreateTs = DateTime.UtcNow;
                            walletModel = _mapper.Map<WalletModel>(item.walletDto);
                            var walletId = await _session.SaveAsync(walletModel);
                            _consumerWalletLogger.LogInformation("{className}.{methodName}: Successfully Created data from  WalletDto: {WalletId}", className, methodName, walletId);
                        }

                        item.consumerWalletDto.WalletId = walletModel.WalletId;
                        item.consumerWalletDto.CreateUser = "SYSTEM";
                        item.consumerWalletDto.CreateTs = DateTime.UtcNow;
                        var consumerWalletModel = _mapper.Map<ConsumerWalletModel>(item.consumerWalletDto);

                        await _session.SaveAsync(consumerWalletModel);

                        _consumerWalletLogger.LogInformation("{className}.{methodName}: Successfully Created data from  ConsumerWalletDto: {ConsumerWalletId}", className, methodName,
                          consumerWalletModel.ConsumerWalletId);

                        await transaction.CommitAsync();

                        responseDtos.Add(new ConsumerWalletDataResponseDto()
                        {
                            Wallet = new WalletDto()
                            {
                                WalletId = walletModel.WalletId,
                                WalletCode = walletModel.WalletCode
                            },
                            ConsumerWallet = new ConsumerWalletDto()
                            {
                                ConsumerCode = consumerWalletModel.ConsumerCode,
                                ConsumerWalletId = consumerWalletModel.ConsumerWalletId
                            }
                        });
                    }

                    catch (Exception ex)
                    {
                        _consumerWalletLogger.LogError(ex, "{className}.{methodName}: ERROR Msg:{msg}, Error Code:{errorCode}", className, methodName, ex.Message, StatusCodes.Status500InternalServerError);
                        await transaction.RollbackAsync();
                    }
                }
                return responseDtos;
            }

            catch (Exception ex)
            {
                _consumerWalletLogger.LogError(ex, "{className}.{methodName}: Error Msg:{errorMsg}", className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get all wallets for given consumer code 
        /// </summary>
        /// <param name="consumerWalletRequestDto"></param>
        /// <returns></returns>

        public async Task<ConsumerWalletResponseDto> GetAllConsumerWalletsAsync(GetConsumerWalletRequestDto consumerWalletRequestDto)
        {
            const string methodName = nameof(GetAllConsumerWalletsAsync);
            try
            {
                var responseDto = new ConsumerWalletResponseDto();

                var consumerWallets = await _consumerWalletRepo.GetConsumerAllWallets(consumerWalletRequestDto.TenantCode, consumerWalletRequestDto.ConsumerCode);
                if (consumerWallets.Count != 0)
                {
                    responseDto.ConsumerWalletDetails = _mapper.Map<List<ConsumerWalletDetailDto>>(consumerWallets);
                }
                else
                {
                    responseDto.ErrorCode = StatusCodes.Status404NotFound;
                    responseDto.ErrorMessage = $"No wallets found for ConsumerCode: {consumerWalletRequestDto.ConsumerCode}";
                }

                return responseDto;
            }

            catch (Exception ex)
            {
                _consumerWalletLogger.LogError(ex, "{className}.{methodName}: Error Msg:{errorMsg}", className, methodName, ex.Message);
                throw;
            }
        }

        /// <summary>
        /// Get all wallets for given consumer code 
        /// </summary>
        /// <param name="consumerWalletRequestDto"></param>
        /// <returns></returns>

        public async Task<ConsumerWalletResponseDto> GetAllConsumerRedeemableWalletsAsync(FindConsumerWalletRequestDto consumerWalletRequestDto)
        {
            const string methodName = nameof(GetAllConsumerWalletsAsync);
            try
            {
                var responseDto = new ConsumerWalletResponseDto();

                var consumerWallets = await _consumerWalletRepo.GetConsumerWalletsWithDetails(consumerWalletRequestDto.ConsumerCode ?? string.Empty, consumerWalletRequestDto.IncludeRedeemOnlyWallets);
                if (consumerWallets.Count != 0)
                {
                    responseDto.ConsumerWalletDetails = _mapper.Map<List<ConsumerWalletDetailDto>>(consumerWallets);
                }
                else
                {
                    responseDto.ErrorCode = StatusCodes.Status404NotFound;
                    responseDto.ErrorMessage = $"No wallets found for ConsumerCode: {consumerWalletRequestDto.ConsumerCode}";
                }

                return responseDto;
            }

            catch (Exception ex)
            {
                _consumerWalletLogger.LogError(ex, "{className}.{methodName}: Error Msg:{errorMsg}", className, methodName, ex.Message);
                throw;
            }
        }
    }
}





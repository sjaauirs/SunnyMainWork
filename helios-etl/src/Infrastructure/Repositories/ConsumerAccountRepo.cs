using AutoMapper;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.ETL.Common.Repositories;
using SunnyRewards.Helios.ETL.Core.Domain.Models;
using SunnyRewards.Helios.ETL.Infrastructure.Repositories.HeliosRepo.Interfaces;

namespace SunnyRewards.Helios.ETL.Infrastructure.Repositories
{
    public class ConsumerAccountRepo : BaseRepo<ETLConsumerAccountModel>, IConsumerAccountRepo
    {
        private readonly NHibernate.ISession _session;
        private readonly IMapper _mapper;
        public ConsumerAccountRepo(ILogger<BaseRepo<ETLConsumerAccountModel>> baseLogger, NHibernate.ISession session, IMapper mapper) : base(baseLogger, session)
        {
            _session = session;
            _mapper = mapper;
        }

        public List<ETLConsumerAccountModel> GetConsumerAccounts(string tenantCode, int take)
        {
            var consumerAccounts = _session.Query<ETLConsumerAccountModel>()
                                  .Where(x => x.TenantCode == tenantCode && x.DeleteNbr == 0 && x.SyncRequired && x.SyncInfoJson != null)
                                  .Select(x => x).OrderBy(x => x.ConsumerAccountId).Take(take).ToList();
            return consumerAccounts;
        }

        /// <summary>
        /// Strictly When ever we are updating consumerAccount don't use UpdateAsync instead use this method. So
        /// It will update consumerAccount and with the same object will Update ConsumerAccountHistory also.
        /// </summary>
        /// <param name="consumerAccountModel"></param>
        /// <returns></returns>
        public async Task<ETLConsumerAccountModel> UpdateConsumerAccount(ETLConsumerAccountModel consumerAccountModel)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                await _session.UpdateAsync(consumerAccountModel);
                var consumerAccountHistoryModel = _mapper.Map<ETLConsumerAccountHistoryModel>(consumerAccountModel);
                await SaveConsumerAccountHistory(consumerAccountHistoryModel);

                await transaction.CommitAsync();
                return consumerAccountModel;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _session.Clear();
                throw;
            }
        }
        private async Task<ETLConsumerAccountHistoryModel> SaveConsumerAccountHistory(ETLConsumerAccountHistoryModel consumerAccountHistoryModel)
        {
            try
            {
                consumerAccountHistoryModel.CreateUser = consumerAccountHistoryModel.UpdateUser ?? consumerAccountHistoryModel.CreateUser;
                consumerAccountHistoryModel.CreateTs = DateTime.UtcNow;
                await _session.SaveAsync(consumerAccountHistoryModel);
                return consumerAccountHistoryModel;
            }
            catch (Exception ex)
            {
                throw new InvalidDataException(ex.Message);
            }
        }
    }
}

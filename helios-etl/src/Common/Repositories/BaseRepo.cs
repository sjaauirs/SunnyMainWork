using Microsoft.Extensions.Logging;
using NHibernate;
using NHibernate.Linq;
using SunnyRewards.Helios.ETL.Common.Domain.Models;
using SunnyRewards.Helios.ETL.Common.Extensions;
using SunnyRewards.Helios.ETL.Common.Repositories.Interfaces;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.ETL.Common.Repositories
{
    public class BaseRepo<T> : IBaseRepo<T> where T : BaseModel
    {
        /// <summary>
        /// This will be injected in .net core dependencies and will automatically auto wired here.
        /// </summary>

        private readonly ILogger<BaseRepo<T>> _baseLogger;
        private readonly ISession _session;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseLogger"></param>
        /// <param name="session"></param>
        public BaseRepo(ILogger<BaseRepo<T>> baseLogger, ISession session)
        {
            _baseLogger = baseLogger;
            _session = session;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        private IQueryable<T> GetQuery(bool includeDeleted = false)
        {
            return includeDeleted ? _session.Query<T>() : _session.Query<T>().Where(x => x.DeleteNbr == 0);
        }

        /// <summary>
        /// Get one record from DB using primary key. it will always look for active records unless 
        /// you want inactive then switch of active parameter
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public async Task<T?> FindOneAsync(long Id)
        {
            try
            {
                var model = await _session.GetAsync<T>(Id);

                if (model != null && model.DeleteNbr == 0)
                    return model;

                return default;
            }
            catch (Exception ex)
            {
                _baseLogger.LogError(ex, "BaseRepo FindOneAsync Error :");
                throw;
            }
        }

        /// <summary>
        /// Find one db record by using expression of query.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        public async Task<T> FindOneAsync(Expression<Func<T, bool>> expression, bool includeDeleted = false)
        {
            try
            {
                return await LinqExtensionMethods.SingleOrDefaultAsync<T>(GetQuery(includeDeleted).Where(expression));
            }
            catch (Exception ex)
            {
                _baseLogger.LogError(ex, "BaseRepo FindOneAsync Error :");
                throw;
            }

        }

        /// <summary>
        /// Find db record by using expression of query.
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        public async Task<IList<T>> FindAsync(Expression<Func<T, bool>> expression, bool includeDeleted = false)
        {
            try
            {
                return await GetQuery(includeDeleted).Where(expression).ToListAsync();
            }
            catch (Exception ex)
            {
                _baseLogger.LogError(ex, "BaseRepo FindAsync Error :");
                throw;
            }
        }

        /// <summary>
        /// Same as FindAsync but allows an order by expression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="orderByExpression"></param>
        /// <param name="descending"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        public async Task<IList<T>> FindOrderedAsync(Expression<Func<T, bool>> expression, Expression<Func<T, bool>> orderByExpression, bool descending, bool includeDeleted = false)
        {
            try
            {
                return descending ? await GetQuery(includeDeleted).Where(expression).OrderByDescending(orderByExpression).ToListAsync() :
                    await GetQuery(includeDeleted).Where(expression).OrderBy(orderByExpression).ToListAsync();
            }
            catch (Exception ex)
            {
                _baseLogger.LogError(ex, "BaseRepo FindOrderedAsync Error :");
                throw;
            }
        }


        /// <summary>
        /// Will get all the records for particular table/model
        /// </summary>
        /// <returns></returns>
        public async Task<IList<T>> FindAllAsync()
        {
            try
            {
                return await _session.Query<T>().ToListAsync();
            }
            catch (Exception ex)
            {
                _baseLogger.LogError(ex, "BaseRepo FindAllAsync Error :");
                throw;
            }
        }

        /// <summary>
        /// Create/persist a record in db
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<T> CreateAsync(T model)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                if (model == null)
                    throw new ArgumentNullException(nameof(model));

                await _session.SaveAsync(model);
                await _session?.FlushAsync();
                await transaction.CommitAsync();

                return model;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _session.Clear();
                _baseLogger.LogError(ex, "BaseRepo CreateAsync Error :");
                throw;
            }
        }

        /// <summary>
        /// Will update model/records in DB
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task<T> UpdateAsync(T model)
        {
            using var transaction = _session.BeginTransaction();
            try
            {
                ArgumentNullException.ThrowIfNull(model);

                await _session.UpdateAsync(model);
                await transaction.CommitAsync();

                return model;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _session.Clear();
                _baseLogger.LogError(ex, "BaseRepo UpdateAsync request:{request} Error :", model.ToJson());
                throw;
            }
        }

        /// <summary>
        /// Delete a particular record from DB, it will be a soft delete will only set
        /// ActiveInd = false
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task DeleteAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new ArgumentOutOfRangeException(nameof(id), "Id must be greater than zero");

                var model = await FindOneAsync(id);
                if (model != null)
                {
                    await UpdateAsync(model);
                }
            }
            catch (Exception ex)
            {
                _baseLogger.LogError(ex, "BaseRepo DeleteAsync id:{id} Error :", id);
                throw;
            }
        }
    }
}

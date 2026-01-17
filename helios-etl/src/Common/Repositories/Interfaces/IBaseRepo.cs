using SunnyRewards.Helios.ETL.Common.Domain.Models;
using System.Linq.Expressions;

namespace SunnyRewards.Helios.ETL.Common.Repositories.Interfaces
{
    public interface IBaseRepo<T> where T : BaseModel
    {
        /// <summary>
        /// Find one db record using id
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        Task<T?> FindOneAsync(long Id);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        Task<T> FindOneAsync(Expression<Func<T, bool>> expression, bool includeDeleted = false);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        Task<IList<T>> FindAsync(Expression<Func<T, bool>> expression, bool includeDeleted = false);

        /// <summary>
        /// Same as FindAsync but allows an order by expression
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="orderByExpression"></param>
        /// <param name="descending"></param>
        /// <param name="includeDeleted"></param>
        /// <returns></returns>
        Task<IList<T>> FindOrderedAsync(Expression<Func<T, bool>> expression, Expression<Func<T, bool>> orderByExpression, bool descending, bool includeDeleted = false);

        /// <summary>
        /// Will get all the records for particular table/model
        /// </summary>
        /// <returns></returns>
        Task<IList<T>> FindAllAsync();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<T> CreateAsync(T model);

        /// <summary>
        /// Update a particular record in DB
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        Task<T> UpdateAsync(T model);

        /// <summary>
        /// Delete a particular record from DB, it will be a soft delete will only set
        /// ActiveInd = false
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteAsync(int id);
    }
}

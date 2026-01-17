using Microsoft.Extensions.Logging;
using NHibernate.Linq;
using SunnyRewards.Helios.Common.Core.Repositories;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;
using SunnyRewards.Helios.User.Infrastructure.ReadReplica;
using SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces;

namespace SunnyRewards.Helios.User.Infrastructure.Repositories
{
    public class PersonRepo : BaseRepo<PersonModel>, IPersonRepo
    {
        private readonly NHibernate.ISession _session;
        private readonly IReadOnlySession? _readOnlySession;

        public PersonRepo(
            ILogger<BaseRepo<PersonModel>> baseLogger,
            NHibernate.ISession session,
            IReadOnlySession? readOnlySession = null) : base(baseLogger, session)
        {
            _session = session;
            _readOnlySession = readOnlySession;
        }

        private NHibernate.ISession ReadSession => _readOnlySession?.Session ?? _session;
        /// <summary>
        /// Retrieves a list of consumers and persons based on the tenant code and optional search term.
        /// </summary>
        /// <param name="tenantCode">The tenant code used to filter consumers.</param>
        /// <param name="searchTerm">Optional search term to filter by person details such as name or email.</param>
        /// <param name="skip">The number of records to skip for pagination.</param>
        /// <param name="take">The number of records to take for pagination.</param>
        /// <returns>A paginated list of consumer and person models that match the provided criteria.</returns>
        public async Task<List<ConsumersAndPersonsModels>> GetConsumerPersons(string tenantCode, string? searchTerm, int skip, int take)
        {
            try
            {
                // Use read replica for this read-only operation
                var session = ReadSession;
                var query = from c in session.Query<ConsumerModel>()
                            join p in session.Query<PersonModel>() on c.PersonId equals p.PersonId
                            where c.TenantCode == tenantCode && c.DeleteNbr == 0 && p.DeleteNbr == 0
                            select new { Person = p, Consumer = c };


//Search
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var lowerSearchTerm = searchTerm.ToLower();
                    query = query.Where(q =>
                        (q.Person.FirstName != null && q.Person.FirstName.ToLower().Contains(lowerSearchTerm)) ||
                        (q.Person.LastName != null && q.Person.LastName.ToLower().Contains(lowerSearchTerm)) ||
                        (q.Person.Email != null && q.Person.Email.ToLower().Contains(lowerSearchTerm))
                    );
                }

//Paging
                query = query.OrderBy(q => q.Person.PersonId) // Ordering by PersonId to ensure stability
                             .Skip(skip)
                             .Take(take);


                var result = await query
           .Select(q => new ConsumersAndPersonsModels(q.Person, q.Consumer))
           .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error in querying Consumers and persons for Tenantcode : {tenantCode}";
                throw new InvalidOperationException(errorMsg, ex);
            }
        }

        public async Task<List<ConsumersAndPersonsModels>> GetConsumerPersons(List<string> consumerCodes , string tenantCode)
        {
            try
            {
                // Use read replica for this read-only operation
                var session = ReadSession;
                var query = from c in session.Query<ConsumerModel>()
                            join p in session.Query<PersonModel>() on c.PersonId equals p.PersonId
                            where c.TenantCode == tenantCode && c.DeleteNbr == 0 && p.DeleteNbr == 0
                            && consumerCodes.Contains(c.ConsumerCode!)
                            select new { Person = p, Consumer = c };

                var result = await query
           .Select(q => new ConsumersAndPersonsModels(q.Person, q.Consumer))
           .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error in querying Consumers and persons for Tenantcode : {tenantCode}";
                throw new InvalidOperationException(errorMsg, ex);
            }
        }

        public async Task<List<ConsumersAndPersonsModels>> GetConsumerPersonsByDOB(GetConsumerByTenantCodeAndDOBRequestDto requestDto)
        {
            try
            {
                // Use read replica for this read-only operation
                var session = ReadSession;
                var query = from c in session.Query<ConsumerModel>()
                            join p in session.Query<PersonModel>() on c.PersonId equals p.PersonId
                            where c.TenantCode == requestDto.TenantCode && c.DeleteNbr == 0 && p.DeleteNbr == 0
                            && p.DOB.Date == requestDto.DOB.Value.Date
                            select new { Person = p, Consumer = c };

                var result = await query
                   .Select(q => new ConsumersAndPersonsModels(q.Person, q.Consumer))
                   .ToListAsync();

                return result;
            }
            catch (Exception ex)
            {
                var errorMsg = $"Error in querying Consumers and persons for Tenantcode : {requestDto.TenantCode}";
                throw new InvalidOperationException(errorMsg, ex);
            }
        }
    }
}
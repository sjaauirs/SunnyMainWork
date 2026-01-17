using SunnyRewards.Helios.Common.Core.Repositories.Interfaces;
using SunnyRewards.Helios.User.Core.Domain.Dtos;
using SunnyRewards.Helios.User.Core.Domain.Models;

namespace SunnyRewards.Helios.User.Infrastructure.Repositories.Interfaces
{
    public interface IPersonRepo : IBaseRepo<PersonModel>
    {
        /// <summary>
        /// Retrieves a list of consumers and persons based on the tenant code and optional search term.
        /// </summary>
        /// <param name="tenantCode">The tenant code used to filter consumers.</param>
        /// <param name="searchTerm">Optional search term to filter by person details such as name or email.</param>
        /// <param name="skip">The number of records to skip for pagination.</param>
        /// <param name="take">The number of records to take for pagination.</param>
        /// <returns>A paginated list of consumer and person models that match the provided criteria.</returns>
        Task<List<ConsumersAndPersonsModels>> GetConsumerPersons(string tenantCode, string? searchTerm, int skip, int take);

        Task<List<ConsumersAndPersonsModels>> GetConsumerPersons(List<string> consumerCodes, string tenantCode);

        Task<List<ConsumersAndPersonsModels>> GetConsumerPersonsByDOB(GetConsumerByTenantCodeAndDOBRequestDto requestDto);
    }
}
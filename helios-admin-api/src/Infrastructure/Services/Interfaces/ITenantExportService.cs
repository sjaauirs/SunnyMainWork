using SunnyRewards.Helios.Admin.Core.Domain.Dtos;

namespace SunnyRewards.Helios.Admin.Infrastructure.Services.Interfaces
{
    public interface ITenantExportService
    {
        /// <summary>
        /// Exports tenant data based on the provided request.
        /// </summary>
        /// <param name="request">The export tenant request DTO containing the necessary parameters.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the export tenant response DTO.</returns>
        Task<ExportTenantResponseDto> ExportTenantAsync(ExportTenantRequestDto request);
    }

}

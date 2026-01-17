using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SunnyRewards.Helios.Tenant.Core.Domain.Dtos;
using SunnyRewards.Helios.Tenant.Infrastructure.Services.Interfaces;

namespace SunnyRewards.Helios.Tenant.Api.Controllers
{
    [ApiController]
    [Route("api/v1/wallet-category")]
    public class WalletCategoryController : ControllerBase
    {
        private readonly IWalletCategoryService _walletCategoryService;
        private readonly IMapper _mapper;
        private readonly ILogger<WalletCategoryController> _logger;

        public WalletCategoryController(
            IWalletCategoryService walletCategoryService,
            IMapper mapper,
            ILogger<WalletCategoryController> logger)
        {
            _walletCategoryService = walletCategoryService;
            _mapper = mapper;
            _logger = logger;
        }

        /// <summary>
        /// Get all wallet categories for a given tenant code.
        /// </summary>
        [HttpGet("tenant/{tenantCode}")]
        public async Task<IActionResult> GetByTenantCode(string tenantCode)
        {
            const string methodName = nameof(GetByTenantCode);
            try
            {
                var result = await _walletCategoryService.GetByTenantCodeAsync(tenantCode);

                if (result == null || !result.Any())
                {
                    _logger.LogWarning("{methodName}: No wallet categories found for tenant {tenantCode}", methodName, tenantCode);
                    return NotFound();
                }

                var dto = _mapper.Map<IEnumerable<WalletCategoryResponseDto>>(result);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{methodName}: Exception occurred for tenant {tenantCode}", methodName, tenantCode);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Get a wallet category by Id.
        /// </summary>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            const string methodName = nameof(GetById);
            try
            {
                var result = await _walletCategoryService.GetByIdAsync(id);

                if (result == null)
                {
                    _logger.LogWarning("{methodName}: Wallet category with id {id} not found", methodName, id);
                    return NotFound();
                }

                var dto = _mapper.Map<WalletCategoryResponseDto>(result);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{methodName}: Exception occurred for wallet category id {id}", methodName, id);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }

        /// <summary>
        /// Get all wallet categories for a given tenant and walletTypeId.
        /// </summary>
        [HttpGet("tenant/{tenantCode}/wallet/{walletTypeId:long}")]
        public async Task<IActionResult> GetByTenantAndWalletTypeId(string tenantCode, long walletTypeId)
        {
            const string methodName = nameof(GetByTenantAndWalletTypeId);
            try
            {
                var result = await _walletCategoryService.GetByTenantCodeAsync(tenantCode);

                if (result == null || !result.Any())
                {
                    _logger.LogWarning("{methodName}: No wallet categories found for tenant {tenantCode}", methodName, tenantCode);
                    return NotFound();
                }

                var filtered = result.Where(x => x.WalletTypeId == walletTypeId);

                if (!filtered.Any())
                {
                    _logger.LogWarning("{methodName}: No wallet categories found for tenant {tenantCode} with walletTypeId {walletTypeId}", methodName, tenantCode, walletTypeId);
                    return NotFound();
                }

                var dto = _mapper.Map<IEnumerable<WalletCategoryResponseDto>>(filtered);
                return Ok(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{methodName}: Exception occurred for tenant {tenantCode} and walletTypeId {walletTypeId}", methodName, tenantCode, walletTypeId);
                return StatusCode(500, "An error occurred while processing your request.");
            }
        }
    }
}

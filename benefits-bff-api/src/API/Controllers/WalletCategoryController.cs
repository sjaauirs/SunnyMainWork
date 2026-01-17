using Microsoft.AspNetCore.Mvc;
using Sunny.Benefits.Bff.Infrastructure.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using SunnyBenefits.Fis.Core.Domain.Dtos;
namespace Sunny.Benefits.Bff.Api.Controllers
{
    [Route("api/v1/wallet-category")]
    [ApiController]
    [Authorize]
    public class WalletCategoryController : ControllerBase
    {
        private readonly ILogger<WalletCategoryController> _logger;
        private readonly IWalletCategoryService _walletCategoryService;
        private const string className = nameof(WalletCategoryController);

        public WalletCategoryController(
            ILogger<WalletCategoryController> logger,
            IWalletCategoryService walletCategoryService)
        {
            _logger = logger;
            _walletCategoryService = walletCategoryService;
        }

        [HttpGet("tenant/{tenantCode}")]
        public async Task<ActionResult> GetByTenant([FromRoute] string tenantCode)
        {
            const string methodName = nameof(GetByTenant);
            _logger.LogInformation("{ClassName}.{MethodName} started for TenantCode:{TenantCode}",
                className, methodName, tenantCode);

            try
            {
                var response = await _walletCategoryService.GetByTenant(tenantCode);
                if (response == null || !response.Any())
                {
                    return NotFound();
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} failed for TenantCode:{TenantCode}",
                    className, methodName, tenantCode);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("{id:long}")]
        public async Task<ActionResult> GetById([FromRoute] long id)
        {
            const string methodName = nameof(GetById);
            _logger.LogInformation("{ClassName}.{MethodName} started for Id:{Id}",
                className, methodName, id);

            try
            {
                var response = await _walletCategoryService.GetById(id);
                if (response == null)
                {
                    return NotFound();
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} failed for Id:{Id}",
                    className, methodName, id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpGet("tenant/{tenantCode}/wallet/{walletTypeId:long}")]
        public async Task<ActionResult> GetByTenantAndWallet([FromRoute] string tenantCode, [FromRoute] long walletTypeId)
        {
            const string methodName = nameof(GetByTenantAndWallet);
            _logger.LogInformation("{ClassName}.{MethodName} started for TenantCode:{TenantCode}, WalletTypeId:{WalletTypeId}",
                className, methodName, tenantCode, walletTypeId);

            try
            {
                var response = await _walletCategoryService.GetByTenantAndWallet(tenantCode, walletTypeId);
                if (response == null)
                {
                    return NotFound();
                }
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "{ClassName}.{MethodName} failed for TenantCode:{TenantCode}, WalletTypeId:{WalletTypeId}",
                    className, methodName, tenantCode, walletTypeId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }
    }
}

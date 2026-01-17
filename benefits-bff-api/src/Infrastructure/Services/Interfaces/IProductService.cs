using SunnyBenefits.Fis.Core.Domain.Dtos;

namespace Sunny.Benefits.Bff.Infrastructure.Services.Interfaces
{
    public interface IProductService
    {
        Task<ProductSearchResponseDto> SearchProduct(PostSearchProductRequestDto searchProductRequestDto);
    }
}

using Mango.Web.Models;

namespace Mango.Web.Services.IService
{
    public interface IProductService
    {
        Task<ResponseDto?> GetProductAsync(string code);
        Task<ResponseDto?> GetAllProductsAsync();
        Task<ResponseDto?> GetProductByIdAsync(int id);
        Task<ResponseDto?> CreateProductAsync(ProductDto couponDto);
        Task<ResponseDto?> UpdateProductAsync(ProductDto couponDto);
        Task<ResponseDto?> DeleteProductAsync(int id);
    }
}
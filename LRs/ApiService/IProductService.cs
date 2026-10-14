using WEB_453505_Shirko.Domain.Entities;
using WEB_453505_Shirko.Domain.Models;

namespace LRs.ApiService
{
    public interface IProductService
    {
        Task<ResponseData<Bike>> GetProductByIdAsync(int id);
        Task<ResponseData<List<Category>>> GetCategoriesAsync();
        Task<ResponseData<ListModel<Bike>>> GetProductListAsync(
            string? categoryNormalizedName,
            int pageNo = 1);
        Task<ResponseData<Bike>> CreateProductAsync(
            Bike product,
            IFormFile? formFile);

        Task UpdateProductAsync(
            int id,
            Bike product,
            IFormFile? formFile);
        Task DeleteProductAsync(int id);
    }
}
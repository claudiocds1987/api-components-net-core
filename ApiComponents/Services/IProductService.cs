using ApiComponents.Models;

namespace ApiComponents.Services
{
    public interface IProductService
    {
        Task ProcessExcelAsync(IFormFile file);
        Task<Product> GetProductByIdAsync(int id);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int id);
    }
}
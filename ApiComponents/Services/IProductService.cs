namespace ApiComponents.Services;

using ApiComponents.Models;
public interface IProductService
{
    Task ProcessExcelAsync(IFormFile file);
    Task<Product> GetProductByIdAsync(int id);
    Task<object> GetAllProductsAsync(int? page, int? size);  // retorno a un objeto para que Angular entienda fácilmente
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(int id);
}
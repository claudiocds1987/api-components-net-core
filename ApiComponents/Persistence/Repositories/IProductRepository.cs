using ApiComponents.Models;

namespace ApiComponents.Persistence.Repositories
{
    public interface IProductRepository
    {

        Task AddProductsList(List<Product> products);
        Task<Product> GetProduct(int id);
        Task<(List<Product> Items, int TotalCount)> GetProductsAsync(int? page, int? size);  // Si page y size son null, trae todos sin paginar.
        Task DeleteProduct(int id);
        Task UpdateProduct(Product product);
        Task<bool> ExistProduct(string title); // Validamos por título ya que el ID es automático
    }
}

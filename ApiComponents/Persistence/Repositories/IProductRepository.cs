using ApiComponents.Models;

namespace ApiComponents.Persistence.Repositories
{
    public interface IProductRepository
    {

        Task AddProductsList(List<Product> products);
        Task<Product> GetProduct(int id);
        Task DeleteProduct(int id);
        Task UpdateProduct(Product product);
        Task<bool> ExistProduct(string title); // Validamos por título ya que el ID es automático
    }
}

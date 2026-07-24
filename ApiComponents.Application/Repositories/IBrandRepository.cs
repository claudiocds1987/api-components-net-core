using ApiComponents.Domain.Models;

namespace ApiComponents.Application.Repositories;

public interface IBrandRepository
{
    Task<bool> ExistBrandAsync(string name, CancellationToken cancellationToken = default);
    Task<IEnumerable<ProductBrand>> GetAllBrandsAsync(bool? isActive = true, CancellationToken cancellationToken = default);
    Task<ProductBrand?> GetBrandByIdAsync(int id, CancellationToken cancellationToken = default);
    Task CreateBrandAsync(ProductBrand brand, CancellationToken cancellationToken = default);
    Task UpdateBrandAsync(ProductBrand brand, CancellationToken cancellationToken = default);
    Task DeleteBrandAsync(int id, CancellationToken cancellationToken = default);
}
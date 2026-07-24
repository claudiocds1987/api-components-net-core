using ApiComponents.Application.DTOs;

namespace ApiComponents.Application.Repositories
{
    public interface IBrandRepository
    {
        Task<IEnumerable<BrandResponseDTo>> GetAllBrandsAsync(bool? isActive = true, CancellationToken cancellationToken = default);
        Task<BrandResponseDTo?> GetBrandAsync(int id, CancellationToken cancellationToken = default);
        Task CreateBrandAsync(BrandRequestDTo brand, CancellationToken cancellationToken = default);
        Task UpdateBrandAsync(BrandRequestDTo brand, CancellationToken cancellationToken = default);
        Task DeleteBrandAsync(int id, CancellationToken cancellationToken = default);
        Task<bool> ExistBrandAsync(string name, CancellationToken cancellationToken = default);
    }
}

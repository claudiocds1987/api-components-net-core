using ApiComponents.Application.DTOs;

namespace ApiComponents.Services
{
    public interface IMercadoPagoService
    {
        Task<string> CreatePreferenceAsync(CartDto cart, CancellationToken cancellationToken = default);
        Task<string> GetPaymentStatusAsync(string paymentId);
    }
}
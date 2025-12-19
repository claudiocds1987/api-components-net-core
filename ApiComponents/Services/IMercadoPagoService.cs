using ApiComponents.DTOs;
using System.Threading.Tasks;

namespace ApiComponents.Services
{
    public interface IMercadoPagoService
    {
        // Recibe el carrito y devuelve el PreferenceId de MP
        Task<string> CreatePreferenceAsync(CartDto cart);
        Task<string> GetPaymentStatusAsync(string paymentId); // Para validar contra la API de MP
    }
}

using ApiComponents.DTOs;
using System.Threading.Tasks;

namespace ApiComponents.Services
{
    public interface IMercadoPagoService
    {
        // Recibe el carrito y devuelve el PreferenceId de MP
        Task<string> CreatePreferenceAsync(CartDto cart);
    }
}

using ApiComponents.DTOs;
using ApiComponents.Models;
using ApiComponents.Persistence.Repositories;
using ApiComponents.Services;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Resource.Preference;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;

public class MercadoPagoService : IMercadoPagoService
{
    private readonly IConfiguration _configuration;
    private readonly IOrderRepository _orderRepository;

    public MercadoPagoService(IConfiguration configuration, IOrderRepository orderRepository)
    {
        _configuration = configuration;
        _orderRepository = orderRepository;
        MercadoPagoConfig.AccessToken = _configuration["MercadoPago:AccessToken"];
    }

    public async Task<string> CreatePreferenceAsync(CartDto cart)
    {
        var client = new PreferenceClient();

        // 1. Calculamos el total para guardarlo en nuestra DB
        decimal total = cart.Items.Sum(i => i.Price * i.Quantity);

        var request = new PreferenceRequest
        {
            Items = cart.Items.Select(item => new PreferenceItemRequest
            {
                Title = item.Name,
                Quantity = item.Quantity,
                UnitPrice = item.Price,
                CurrencyId = "ARS"
            }).ToList(),
            // ... resto de la configuración (BackUrls, etc.)
        };

        var preference = await client.CreateAsync(request);

        // 2. GUARDAMOS EN NUESTRA BASE DE DATOS
        var order = new Order
        {
            PreferenceId = preference.Id,
            TotalAmount = total,
            Status = "Pending"
        };
        await _orderRepository.AddAsync(order);

        return preference.Id; // Devolvemos el PreferenceId para que el frontend Angular lo use
    }
}

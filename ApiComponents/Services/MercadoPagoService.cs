using ApiComponents.DTOs;
using ApiComponents.Models;
using ApiComponents.Persistence.Repositories;
using ApiComponents.Services;
using MercadoPago.Client.Payment;
using MercadoPago.Client.Preference;
using MercadoPago.Config;
using MercadoPago.Error;
using MercadoPago.Resource.Preference;
using Microsoft.Extensions.Configuration;
using System;
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

        // 1. Cálculo del total para tu DB
        decimal total = cart.Items.Sum(i => i.Price * i.Quantity);

        var request = new PreferenceRequest
        {
            Items = cart.Items.Select(item => new PreferenceItemRequest
            {
                Id = item.Name, // Agregamos un ID de referencia
                Title = item.Name,
                Quantity = (int)item.Quantity, // Cast explícito a int
                UnitPrice = (decimal)item.Price, // Cast explícito a decimal
                CurrencyId = "ARS"
            }).ToList(),

            // CONFIGURACIÓN DE RETORNO (Cambiado a HTTPS para validación)
            BackUrls = new PreferenceBackUrlsRequest
            {
                // Usamos URLs externas para que Mercado Pago no rechace el 'AutoReturn'
                Success = "https://www.google.com",
                Failure = "https://www.google.com",
                Pending = "https://www.google.com"

                // cuando el front tenga estas urls, se deben actualizar aquí Y Borrar las de arriba
                //Success = "http://localhost:4200/pago-exitoso",
                //Failure = "http://localhost:4200/pago-fallido",
                //Pending = "http://localhost:4200/pago-pendiente"
            },

            // Obligatorio que coincida con BackUrls.Success definido
            AutoReturn = "approved",

            // Identificador único para vincular con tu base de datos
            ExternalReference = Guid.NewGuid().ToString()
        };

        try
        {
            // 3. Intento de creación en la API de Mercado Pago
            var preference = await client.CreateAsync(request);

            // 4. Guardado en tu Repositorio
            var order = new Order
            {
                PreferenceId = preference.Id,
                TotalAmount = total,
                Status = "Pending"
            };

            await _orderRepository.AddAsync(order);

            return preference.Id;
        }
        catch (MercadoPagoApiException ex)
        {
            // Esto te ayudará a ver en la consola exactamente qué campo falta si falla
            Console.WriteLine($"Error MP: {ex.ApiError.Message}");
            throw;
        }
    }

    public async Task<string> GetPaymentStatusAsync(string paymentId)
    {
        var client = new PaymentClient();
        var payment = await client.GetAsync(long.Parse(paymentId));
        return payment.Status; // Devuelve "approved", "rejected", etc.
    }
}

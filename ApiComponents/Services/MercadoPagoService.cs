
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
    private readonly string _baseUrl; // Variable para almacenar la URL base

    public MercadoPagoService(IConfiguration configuration, IOrderRepository orderRepository)
    {
        _configuration = configuration;
        _orderRepository = orderRepository;

        // 1. Cargamos configuración
        MercadoPagoConfig.AccessToken = _configuration["MercadoPago:AccessToken"];
        _baseUrl = _configuration["MercadoPago:BaseUrl"]; // Lee de appsettings.json o appsettings.Production.json
    }

    public async Task<string> CreatePreferenceAsync(CartDto cart)
    {
        var client = new PreferenceClient();
        decimal total = cart.Items.Sum(i => i.Price * i.Quantity);

        // 1. Primero creamos el objeto Order SIN ID (la DB lo generará)
        var order = new Order
        {
            TotalAmount = total,
            Status = "Pending"
            // No asignamos Id ni PreferenceId todavía
        };

        // 2. Guardamos en la DB para que se genere el Id numérico
        await _orderRepository.AddAsync(order);
        // Ahora 'order.Id' ya tiene el número (ej: 1, 2, 3...) asignado por SQL Express

        var request = new PreferenceRequest
        {
            Items = cart.Items.Select(item => new PreferenceItemRequest
            {
                Title = item.Name,
                Quantity = (int)item.Quantity,
                UnitPrice = (decimal)item.Price,
                CurrencyId = "ARS"
            }).ToList(),

            BackUrls = new PreferenceBackUrlsRequest
            {
                Success = "https://claudiocds1987.github.io/angular-ecommerce-v20/payment-result",
                Failure = "https://claudiocds1987.github.io/angular-ecommerce-v20/payment-result",
                Pending = "https://claudiocds1987.github.io/angular-ecommerce-v20/payment-result"
            },
            AutoReturn = "approved",

            // Usamos el ID numérico convertido a string para Mercado Pago
            ExternalReference = order.Id.ToString(),

            NotificationUrl = $"{_baseUrl}/api/MercadoPago/webhook",
        };

        try
        {
            var preference = await client.CreateAsync(request);

            // 3. Actualizamos la orden con el PreferenceId que nos dio MP
            order.PreferenceId = preference.Id;
            await _orderRepository.UpdateStatusAsync(order.PreferenceId, "Pending"); // O un método Update similar

            return preference.Id; // este es el PreferenceId que recibe en el frontend
        }
        catch (MercadoPagoApiException ex)
        {
            Console.WriteLine($"Error MP: {ex.ApiError.Message}");
            throw;
        }
    }

    public async Task<string> GetPaymentStatusAsync(string paymentId)
    {
        // MP a veces manda IDs que pueden ser muy largos, usamos long.Parse
        var client = new PaymentClient();
        var payment = await client.GetAsync(long.Parse(paymentId));

        // Aquí podrías retornar más datos si quisieras (ej: payment.ExternalReference)
        return payment.Status;
    }
}

//using ApiComponents.DTOs;
//using ApiComponents.Models;
//using ApiComponents.Persistence.Repositories;
//using ApiComponents.Services;
//using MercadoPago.Client.Payment;
//using MercadoPago.Client.Preference;
//using MercadoPago.Config;
//using MercadoPago.Error;
//using MercadoPago.Resource.Preference;
//using Microsoft.Extensions.Configuration;
//using System;
//using System.Linq;
//using System.Threading.Tasks;

//public class MercadoPagoService : IMercadoPagoService
//{
//    private readonly IConfiguration _configuration;
//    private readonly IOrderRepository _orderRepository;

//    public MercadoPagoService(IConfiguration configuration, IOrderRepository orderRepository)
//    {
//        _configuration = configuration;
//        _orderRepository = orderRepository;
//        MercadoPagoConfig.AccessToken = _configuration["MercadoPago:AccessToken"];
//    }

//    public async Task<string> CreatePreferenceAsync(CartDto cart)
//    {
//        var client = new PreferenceClient();

//        // 1. Cálculo del total para tu DB
//        decimal total = cart.Items.Sum(i => i.Price * i.Quantity);

//        var request = new PreferenceRequest
//        {
//            Items = cart.Items.Select(item => new PreferenceItemRequest
//            {
//                Id = item.Name, // Agregamos un ID de referencia
//                Title = item.Name,
//                Quantity = (int)item.Quantity, // Cast explícito a int
//                UnitPrice = (decimal)item.Price, // Cast explícito a decimal
//                CurrencyId = "ARS"
//            }).ToList(),

//            // CONFIGURACIÓN DE RETORNO (Cambiado a HTTPS para validación)
//            BackUrls = new PreferenceBackUrlsRequest
//            {
//                // URLs de github pages para resultados de pago
//                Success = "https://claudiocds1987.github.io/angular-ecommerce-v20/payment-result",
//                Failure = "https://claudiocds1987.github.io/angular-ecommerce-v20/payment-result",
//                Pending = "https://claudiocds1987.github.io/angular-ecommerce-v20/payment-result"
//            },

//            // Obligatorio que coincida con BackUrls.Success definido
//            AutoReturn = "approved",

//            // Identificador único para vincular con tu base de datos
//            ExternalReference = Guid.NewGuid().ToString()
//        };

//        try
//        {
//            // 3. Intento de creación en la API de Mercado Pago
//            var preference = await client.CreateAsync(request);

//            // 4. Guardado en tu Repositorio
//            var order = new Order
//            {
//                PreferenceId = preference.Id,
//                TotalAmount = total,
//                Status = "Pending"
//            };

//            await _orderRepository.AddAsync(order);

//            return preference.Id;
//        }
//        catch (MercadoPagoApiException ex)
//        {
//            // Esto te ayudará a ver en la consola exactamente qué campo falta si falla
//            Console.WriteLine($"Error MP: {ex.ApiError.Message}");
//            throw;
//        }
//    }

//    public async Task<string> GetPaymentStatusAsync(string paymentId)
//    {
//        var client = new PaymentClient();
//        var payment = await client.GetAsync(long.Parse(paymentId));
//        return payment.Status; // Devuelve "approved", "rejected", etc.
//    }
//}

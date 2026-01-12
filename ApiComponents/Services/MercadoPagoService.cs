
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

        // --- CORRECCIÓN CRÍTICA ---
        // Fuerza a .NET 8 a usar protocolos modernos de red en el servidor compartido
        System.Net.ServicePointManager.SecurityProtocol =
            System.Net.SecurityProtocolType.Tls12 | System.Net.SecurityProtocolType.Tls13;
        // ---------------------------

        var token = _configuration["MercadoPago:AccessToken"] ?? _configuration["MercadoPago__AccessToken"];
        var baseUrl = _configuration["MercadoPago:BaseUrl"] ?? _configuration["MercadoPago__BaseUrl"];

        if (string.IsNullOrEmpty(token))
        {
            throw new Exception("CRÍTICO: El AccessToken de Mercado Pago no se encontró.");
        }

        MercadoPagoConfig.AccessToken = token;
        _baseUrl = baseUrl ?? "https://apicomponents.runasp.net";
    }

    //public MercadoPagoService(IConfiguration configuration, IOrderRepository orderRepository)
    //{
    //    _configuration = configuration;
    //    _orderRepository = orderRepository;

    //    // Intentamos leer de la sección (appsettings) o directamente de la raíz (Variables de Entorno)
    //    var token = _configuration["MercadoPago:AccessToken"] ?? _configuration["MercadoPago__AccessToken"];
    //    var baseUrl = _configuration["MercadoPago:BaseUrl"] ?? _configuration["MercadoPago__BaseUrl"];

    //    // Validación de seguridad para que el servidor no "muera" en silencio
    //    if (string.IsNullOrEmpty(token))
    //    {
    //        throw new Exception("CRÍTICO: El AccessToken de Mercado Pago no se encontró en las variables de entorno.");
    //    }

    //    MercadoPagoConfig.AccessToken = token;
    //    _baseUrl = baseUrl ?? "https://apicomponents.runasp.net"; // Valor por defecto si falla
    //}



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
            order.PreferenceId = preference.Id;
            await _orderRepository.UpdateStatusAsync(order.PreferenceId, "Pending");
            return preference.Id;
        }
        catch (Exception ex) // Cambiado de MercadoPagoApiException a Exception
        {
            // Esto escribirá el error real en los logs de MonsterASP en lugar de cerrar el proceso
            Console.WriteLine($"ERROR COMPLETO: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"INNER ERROR: {ex.InnerException.Message}");

            throw new Exception($"Error al conectar con Mercado Pago: {ex.Message}", ex);
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

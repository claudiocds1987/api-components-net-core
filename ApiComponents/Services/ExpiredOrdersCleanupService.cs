
using ApiComponents.Application.Features.Orders.Commands;
using MediatR;

namespace ApiComponents.Services;

public class ExpiredOrdersCleanupService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ExpiredOrdersCleanupService> _logger;
    private readonly int _checkIntervalMinutes = 5;

    public ExpiredOrdersCleanupService(IServiceProvider serviceProvider, ILogger<ExpiredOrdersCleanupService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExpiredOrdersCleanupService is starting.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var sender = scope.ServiceProvider.GetRequiredService<ISender>();

                _logger.LogInformation("Verificando órdenes pendientes expiradas...");

                // Buscar y cancelar órdenes con más de 15 minutos en estado Pending
                await sender.Send(new CancelExpiredOrdersCommand(15), stoppingToken);

                _logger.LogInformation("Verificación de órdenes completada.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ocurrió un error cancelando las órdenes expiradas.");
            }

            // Esperar 5 minutos antes de volver a chequear
            await Task.Delay(TimeSpan.FromMinutes(_checkIntervalMinutes), stoppingToken);
        }
    }
}

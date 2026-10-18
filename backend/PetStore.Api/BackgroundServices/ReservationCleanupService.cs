using Microsoft.Extensions.Options;
using PetStore.Domain.Enums;
using PetStore.Domain.Interfaces;
using PetStore.Domain.Settings;

namespace PetStore.Api.BackgroundServices; 

public class ReservationCleanupService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReservationCleanupService> _logger;
    private readonly OrderSettings _settings;

    public ReservationCleanupService(IServiceScopeFactory scopeFactory, ILogger<ReservationCleanupService> logger
        ,IOptions<OrderSettings> settings)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _settings = settings.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                await orderService.CancelExpiredReservationsAsync();
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error while cancelling expired reservations.");
            }
            await Task.Delay(TimeSpan.FromSeconds(_settings.ReservationCheckIntervalSeconds), stoppingToken);
        }
    }
}
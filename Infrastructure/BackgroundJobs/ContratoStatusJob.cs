using Microsoft.EntityFrameworkCore;
using SindiOps.API.Constants;
using SindiOps.API.Helpers;
using SindiOps.API.Infrastructure.Data;

namespace SindiOps.API.Infrastructure.BackgroundJobs;

public class ContratoStatusJob : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ContratoStatusJob> _logger;
    private Timer? _timer;

    public ContratoStatusJob(
        IServiceProvider serviceProvider,
        ILogger<ContratoStatusJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[ContratoStatusJob] Iniciado. Executará a cada 24h.");

        _timer = new Timer(
            callback: async _ => await ExecuteAsync(),
            state: null,
            dueTime: TimeSpan.Zero,
            period: TimeSpan.FromHours(24));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[ContratoStatusJob] Encerrado.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();

    private async Task ExecuteAsync()
    {
        _logger.LogInformation("[ContratoStatusJob] Iniciando atualização de status...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SindiOpsDbContext>();

            var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
            var contratos = await db.Contratos
                .Where(c => c.Status != ContratoStatus.Cancelled)
                .ToListAsync();

            var atualizados = 0;
            foreach (var contrato in contratos)
            {
                if (ContratoStatusCalculator.TryApplyAutomaticStatus(contrato, hoje))
                    atualizados++;
            }

            if (atualizados > 0)
                await db.SaveChangesAsync();

            _logger.LogInformation(
                "[ContratoStatusJob] Concluído em {Data}. Total: {Total} | Atualizados: {Atualizados}",
                hoje, contratos.Count, atualizados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ContratoStatusJob] Erro ao atualizar status de contratos");
        }
    }
}

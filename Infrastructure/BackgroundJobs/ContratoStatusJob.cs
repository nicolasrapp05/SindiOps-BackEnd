using Microsoft.EntityFrameworkCore;
using SindiOps.API.Constants;
using SindiOps.API.Helpers;
using SindiOps.API.Infrastructure.Data;

namespace SindiOps.API.Infrastructure.BackgroundJobs;

public class ContratoStatusJob : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ContratoStatusJob> _logger;

    public ContratoStatusJob(
        IServiceProvider serviceProvider,
        ILogger<ContratoStatusJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[ContratoStatusJob] Iniciado. Executará a cada 24h.");

        using var timer = new PeriodicTimer(Interval);

        await RunOnceAsync(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunOnceAsync(stoppingToken);
        }
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[ContratoStatusJob] Iniciando atualização de status...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SindiOpsDbContext>();

            var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
            var contratos = await db.Contratos
                .Where(c => c.Status != ContratoStatus.Cancelled)
                .ToListAsync(stoppingToken);

            var atualizados = 0;
            foreach (var contrato in contratos)
            {
                if (ContratoStatusCalculator.TryApplyAutomaticStatus(contrato, hoje))
                    atualizados++;
            }

            if (atualizados > 0)
                await db.SaveChangesAsync(stoppingToken);

            _logger.LogInformation(
                "[ContratoStatusJob] Concluído em {Data}. Total: {Total} | Atualizados: {Atualizados}",
                hoje, contratos.Count, atualizados);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("[ContratoStatusJob] Encerrado.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ContratoStatusJob] Erro ao atualizar status de contratos");
        }
    }
}

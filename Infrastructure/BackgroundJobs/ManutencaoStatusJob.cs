using Microsoft.EntityFrameworkCore;
using SindiOps.API.Constants;
using SindiOps.API.Infrastructure.Data;

namespace SindiOps.API.Infrastructure.BackgroundJobs;

public class ManutencaoStatusJob : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromHours(24);

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ManutencaoStatusJob> _logger;

    public ManutencaoStatusJob(
        IServiceProvider serviceProvider,
        ILogger<ManutencaoStatusJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[ManutencaoStatusJob] Iniciado. Executará a cada 24h.");

        using var timer = new PeriodicTimer(Interval);

        await RunOnceAsync(stoppingToken);

        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            await RunOnceAsync(stoppingToken);
        }
    }

    private async Task RunOnceAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[ManutencaoStatusJob] Iniciando atualização de status...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SindiOpsDbContext>();

            var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
            var limiteUpcoming = hoje.AddDays(30);

            var manutencoes = await db.ManutencoesObrigatorias.ToListAsync(stoppingToken);

            var atualizados = 0;
            foreach (var m in manutencoes)
            {
                var novoStatus = m.DataVencimento < hoje
                    ? ManutencaoStatus.Overdue
                    : m.DataVencimento <= limiteUpcoming
                        ? ManutencaoStatus.Upcoming
                        : ManutencaoStatus.Ok;

                if (m.Status != novoStatus)
                {
                    m.Status = novoStatus;
                    atualizados++;
                }
            }

            await db.SaveChangesAsync(stoppingToken);

            _logger.LogInformation(
                "[ManutencaoStatusJob] Concluído em {Data}. Total: {Total} | Atualizados: {Atualizados}",
                hoje, manutencoes.Count, atualizados);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("[ManutencaoStatusJob] Encerrado.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ManutencaoStatusJob] Erro ao atualizar status de manutenções");
        }
    }
}

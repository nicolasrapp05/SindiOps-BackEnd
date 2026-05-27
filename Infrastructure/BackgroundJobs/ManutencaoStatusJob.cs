using Microsoft.EntityFrameworkCore;
using SindiOps.API.Constants;
using SindiOps.API.Infrastructure.Data;

namespace SindiOps.API.Infrastructure.BackgroundJobs;

public class ManutencaoStatusJob : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ManutencaoStatusJob> _logger;
    private Timer? _timer;

    public ManutencaoStatusJob(
        IServiceProvider serviceProvider,
        ILogger<ManutencaoStatusJob> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[ManutencaoStatusJob] Iniciado. Executará a cada 24h.");

        // executa imediatamente ao iniciar, depois repete a cada 24 horas
        _timer = new Timer(
            callback: async _ => await ExecuteAsync(),
            state: null,
            dueTime: TimeSpan.Zero,
            period: TimeSpan.FromHours(24));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[ManutencaoStatusJob] Encerrado.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose() => _timer?.Dispose();

    // ── execução principal ──────────────────────────────────────────────────

    private async Task ExecuteAsync()
    {
        _logger.LogInformation("[ManutencaoStatusJob] Iniciando atualização de status...");

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SindiOpsDbContext>();

            var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
            var limiteUpcoming = hoje.AddDays(30);

            var manutencoes = await db.ManutencoesObrigatorias.ToListAsync();

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

            await db.SaveChangesAsync();

            _logger.LogInformation(
                "[ManutencaoStatusJob] Concluído em {Data}. Total: {Total} | Atualizados: {Atualizados}",
                hoje, manutencoes.Count, atualizados);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ManutencaoStatusJob] Erro ao atualizar status de manutenções");
        }
    }
}

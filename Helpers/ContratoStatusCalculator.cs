using SindiOps.API.Constants;
using SindiOps.API.Entities;

namespace SindiOps.API.Helpers;

public static class ContratoStatusCalculator
{
    public const int ExpiringWindowDays = 30;

    /// <summary>
    /// Calcula status automático com base na data de término.
    /// Contratos cancelados são ignorados (status manual).
    /// </summary>
    public static string ComputeFromDataFim(DateOnly? dataFim, DateOnly hoje)
    {
        if (!dataFim.HasValue)
            return ContratoStatus.Active;

        if (dataFim.Value < hoje)
            return ContratoStatus.Expired;

        if (dataFim.Value <= hoje.AddDays(ExpiringWindowDays))
            return ContratoStatus.Expiring;

        return ContratoStatus.Active;
    }

    /// <summary>
    /// Atualiza o status do contrato quando não está cancelado.
    /// Retorna true se houve alteração.
    /// </summary>
    public static bool TryApplyAutomaticStatus(Contrato contrato, DateOnly? hoje = null)
    {
        if (contrato.Status == ContratoStatus.Cancelled)
            return false;

        hoje ??= DateOnly.FromDateTime(DateTime.UtcNow);
        var novoStatus = ComputeFromDataFim(contrato.DataFim, hoje.Value);

        if (contrato.Status == novoStatus)
            return false;

        contrato.Status = novoStatus;
        contrato.AtualizadoEm = DateTime.UtcNow;
        return true;
    }

    /// <summary>
    /// Define o status calculado ao reativar um contrato cancelado.
    /// </summary>
    public static void ApplyReativacao(Contrato contrato, DateOnly? hoje = null)
    {
        hoje ??= DateOnly.FromDateTime(DateTime.UtcNow);
        contrato.Status = ComputeFromDataFim(contrato.DataFim, hoje.Value);
        contrato.AtualizadoEm = DateTime.UtcNow;
    }
}

using SindiOps.API.Constants;

namespace SindiOps.API.Helpers;

/// <summary>
/// Status da agenda de mandato, alinhado aos filtros do dashboard e ao selo do card.
/// </summary>
public static class MandatoAgenda
{
    public const int WindowDays = 60;

    /// <summary>
    /// <see cref="ManutencaoStatus.Overdue"/> se já venceu,
    /// <see cref="ContratoStatus.Expiring"/> se vence em até 60 dias,
    /// ou nulo quando ainda está vigente fora dessa janela.
    /// </summary>
    public static string? Status(DateOnly vencimento, DateOnly hoje)
    {
        if (vencimento < hoje)
            return ManutencaoStatus.Overdue;
        if (vencimento <= hoje.AddDays(WindowDays))
            return ContratoStatus.Expiring;
        return null;
    }
}

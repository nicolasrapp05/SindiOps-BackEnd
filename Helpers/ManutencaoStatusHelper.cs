using SindiCore.API.Constants;

namespace SindiCore.API.Helpers;

public static class ManutencaoStatusHelper
{
    /// <summary>Alinha com o job diário: hoje em UTC, janela de 30 dias para upcoming.</summary>
    public static string CalcularStatus(DateOnly dataVencimento)
    {
        var hoje = DateOnly.FromDateTime(DateTime.UtcNow);
        var limiteUpcoming = hoje.AddDays(30);

        if (dataVencimento < hoje)
            return ManutencaoStatus.Overdue;
        if (dataVencimento <= limiteUpcoming)
            return ManutencaoStatus.Upcoming;
        return ManutencaoStatus.Ok;
    }
}

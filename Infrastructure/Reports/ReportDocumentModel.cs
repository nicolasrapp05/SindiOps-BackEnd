namespace SindiCore.API.Infrastructure.Reports;

/// <summary>Dados tabulares normalizados para geração de PDF, Excel ou Word.</summary>
public sealed class ReportDocumentModel
{
    public string Titulo { get; init; } = string.Empty;
    public string? Periodo { get; init; }
    public IReadOnlyList<string> Colunas { get; init; } = [];
    public IReadOnlyList<IReadOnlyList<string>> Linhas { get; init; } = [];
}

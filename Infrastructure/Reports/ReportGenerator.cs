using DocumentFormat.OpenXml.Packaging;
using W = DocumentFormat.OpenXml.Wordprocessing;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ClosedXML.Excel;
using SindiOps.API.Constants;

namespace SindiOps.API.Infrastructure.Reports;

public class ReportGenerator : IReportGenerator
{
    static ReportGenerator()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public Task<byte[]> GenerateAsync(string tipo, object dados, string formato)
    {
        if (dados is not ReportDocumentModel model)
            throw new ArgumentException("Formato de dados do relatório inválido.", nameof(dados));

        var bytes = formato.ToLowerInvariant() switch
        {
            RelatorioFormato.Pdf => GeneratePdf(model),
            RelatorioFormato.Excel => GenerateExcel(model),
            RelatorioFormato.Word => GenerateWord(model),
            _ => throw new ArgumentOutOfRangeException(nameof(formato), formato, "Formato não suportado.")
        };

        return Task.FromResult(bytes);
    }

    private static byte[] GeneratePdf(ReportDocumentModel model)
    {
        var colCount = Math.Max(1, model.Colunas.Count);

        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(36);
                page.Size(PageSizes.A4);

                page.Header().Column(col =>
                {
                    col.Spacing(4);
                    col.Item().Text(model.Titulo).FontSize(18).SemiBold();
                    if (!string.IsNullOrWhiteSpace(model.Periodo))
                        col.Item().Text(model.Periodo).FontSize(10).FontColor(Colors.Grey.Darken1);
                });

                page.Content().PaddingTop(16).Table(table =>
                {
                    table.ColumnsDefinition(c =>
                    {
                        for (var i = 0; i < colCount; i++)
                            c.RelativeColumn();
                    });

                    table.Header(h =>
                    {
                        foreach (var header in model.Colunas)
                            h.Cell().Background(Colors.Grey.Lighten3).Padding(6).Text(header).SemiBold();
                    });

                    foreach (var row in model.Linhas)
                    {
                        for (var i = 0; i < colCount; i++)
                        {
                            var text = i < row.Count ? row[i] : string.Empty;
                            table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(6).Text(text ?? string.Empty);
                        }
                    }
                });

                page.Footer().AlignRight().PaddingTop(12)
                    .Text($"Gerado em {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC")
                    .FontSize(9)
                    .FontColor(Colors.Grey.Darken1);
            });
        }).GeneratePdf();
    }

    private static byte[] GenerateExcel(ReportDocumentModel model)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Relatório");
        var r = 1;
        ws.Cell(r, 1).Value = model.Titulo;
        ws.Cell(r, 1).Style.Font.Bold = true;
        r++;
        if (!string.IsNullOrWhiteSpace(model.Periodo))
        {
            ws.Cell(r, 1).Value = model.Periodo;
            r++;
        }

        r++;
        for (var c = 0; c < model.Colunas.Count; c++)
        {
            ws.Cell(r, c + 1).Value = model.Colunas[c];
            ws.Cell(r, c + 1).Style.Font.Bold = true;
        }

        r++;
        foreach (var row in model.Linhas)
        {
            for (var c = 0; c < model.Colunas.Count; c++)
                ws.Cell(r, c + 1).Value = c < row.Count ? row[c] : string.Empty;
            r++;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        return stream.ToArray();
    }

    private static byte[] GenerateWord(ReportDocumentModel model)
    {
        using var ms = new MemoryStream();
        using (var wordDocument = WordprocessingDocument.Create(
                   ms,
                   DocumentFormat.OpenXml.WordprocessingDocumentType.Document,
                   true))
        {
            var mainPart = wordDocument.AddMainDocumentPart();
            mainPart.Document = new W.Document();
            var body = mainPart.Document.AppendChild(new W.Body());

            body.AppendChild(new W.Paragraph(new W.Run(new W.Text(model.Titulo))));
            if (!string.IsNullOrWhiteSpace(model.Periodo))
                body.AppendChild(new W.Paragraph(new W.Run(new W.Text(model.Periodo))));

            var table = new W.Table();

            var headerRow = new W.TableRow();
            foreach (var h in model.Colunas)
            {
                headerRow.Append(new W.TableCell(
                    new W.TableCellProperties(),
                    new W.Paragraph(new W.Run(new W.Text(h)))));
            }

            table.Append(headerRow);

            foreach (var row in model.Linhas)
            {
                var tr = new W.TableRow();
                for (var i = 0; i < model.Colunas.Count; i++)
                {
                    var text = i < row.Count ? row[i] ?? string.Empty : string.Empty;
                    tr.Append(new W.TableCell(
                        new W.TableCellProperties(),
                        new W.Paragraph(new W.Run(new W.Text(text)))));
                }

                table.Append(tr);
            }

            body.AppendChild(table);
            mainPart.Document.Save();
        }

        return ms.ToArray();
    }
}

using SindiOps.API.DTOs.Requests;

namespace SindiOps.API.Services.Interfaces;

public interface IRelatorioService
{
    Task<(byte[] Conteudo, string ContentType, string FileName)> GerarRelatorioAsync(
        GerarRelatorioRequest request,
        Guid userId);
}

using SindiCore.API.DTOs.Requests;

namespace SindiCore.API.Services.Interfaces;

public interface IRelatorioService
{
    Task<(byte[] Conteudo, string ContentType, string FileName)> GerarRelatorioAsync(
        GerarRelatorioRequest request,
        Guid userId);
}

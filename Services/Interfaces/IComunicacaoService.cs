using SindiCore.API.DTOs.Requests;
using SindiCore.API.DTOs.Responses;

namespace SindiCore.API.Services.Interfaces;

public interface IComunicacaoService
{
    /// <summary>
    /// Envia comunicação por e-mail a partir de uma ocorrência.
    /// <paramref name="enviadoPorId"/> é o <c>sub</c> JWT (funcionário do síndico).
    /// </summary>
    Task<ComunicacaoResponse> EnviarComunicacaoAsync(
        Guid ocorrenciaId,
        EnviarComunicacaoRequest request,
        Guid enviadoPorId);
}

using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;

namespace SindiOps.API.Services.Interfaces;

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

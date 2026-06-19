using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;

namespace SindiOps.API.Services.Interfaces;

public interface IAuthService
{
    Task<CadastroSindicoResponse> CadastroSindicoAsync(CadastroSindicoRequest request);
    Task EsqueciSenhaAsync(EsqueciSenhaRequest request);
}

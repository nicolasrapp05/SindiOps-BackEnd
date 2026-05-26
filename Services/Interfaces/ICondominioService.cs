using SindiCore.API.DTOs.Requests;
using SindiCore.API.DTOs.Responses;

namespace SindiCore.API.Services.Interfaces;

public interface ICondominioService
{
    Task<List<CondominioResponse>> GetAllAsync(Guid sindicoId);
    Task<CondominioDetalheResponse> GetByIdAsync(Guid id, Guid sindicoId);
    Task<CondominioResponse> CreateAsync(CreateCondominioRequest request, Guid sindicoId);
    Task<CondominioResponse> UpdateAsync(Guid id, CreateCondominioRequest request, Guid sindicoId);
    Task DeleteAsync(Guid id, Guid sindicoId);
    Task<List<BlocoResponse>> GetBlocosAsync(Guid condominioId, Guid sindicoId);
    Task<BlocoResponse> CreateBlocoAsync(Guid condominioId, CreateBlocoRequest request, Guid sindicoId);
    Task<UnidadeResponse> CreateUnidadeAsync(Guid condominioId, Guid blocoId, CreateUnidadeRequest request, Guid sindicoId);
    Task DeleteBlocoAsync(Guid condominioId, Guid blocoId, Guid sindicoId);
}

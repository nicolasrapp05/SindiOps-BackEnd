using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;

namespace SindiOps.API.Services.Interfaces;

public interface ICondominioService
{
    Task<List<CondominioResponse>> GetAllAsync(Guid sindicoId, Guid userId);
    Task<CondominioDetalheResponse> GetByIdAsync(Guid id, Guid sindicoId, Guid userId);
    Task<CondominioResponse> CreateAsync(CreateCondominioRequest request, Guid sindicoId);
    Task<CondominioResponse> UpdateAsync(Guid id, CreateCondominioRequest request, Guid sindicoId);
    Task DeleteAsync(Guid id, Guid sindicoId);
    Task<List<BlocoResponse>> GetBlocosAsync(Guid condominioId, Guid sindicoId);
    Task<BlocoResponse> CreateBlocoAsync(Guid condominioId, CreateBlocoRequest request, Guid sindicoId);
    Task<UnidadeResponse> CreateUnidadeAsync(Guid condominioId, Guid blocoId, CreateUnidadeRequest request, Guid sindicoId);
    Task DeleteBlocoAsync(Guid condominioId, Guid blocoId, Guid sindicoId);
    Task<BlocoResponse> UpdateBlocoAsync(Guid condominioId, Guid blocoId, UpdateBlocoRequest request, Guid sindicoId);
    Task<UnidadeResponse> UpdateUnidadeAsync(Guid condominioId, Guid blocoId, Guid unidadeId, UpdateUnidadeRequest request, Guid sindicoId);
    Task DeleteUnidadeAsync(Guid condominioId, Guid blocoId, Guid unidadeId, Guid sindicoId);
}

using SindiCore.API.DTOs.Requests;
using SindiCore.API.DTOs.Responses;
using SindiCore.API.Helpers;

namespace SindiCore.API.Services.Interfaces;

public interface ISolicitacaoCompraService
{
    Task<PaginatedResponse<SolicitacaoCompraResponse>> GetAllAsync(Guid userId, SolicitacaoCompraQueryParams queryParams);
    Task<SolicitacaoCompraDetalheResponse> GetByIdAsync(Guid id, Guid userId);
    Task<SolicitacaoCompraResponse> CreateAsync(CreateSolicitacaoCompraRequest request, Guid userId);
    Task<SolicitacaoCompraResponse> UpdateAsync(Guid id, CreateSolicitacaoCompraRequest request, Guid userId);
    Task<SolicitacaoCompraDetalheResponse> AprovarAsync(Guid id, Guid userId);
    Task<SolicitacaoCompraResponse> UpdateStatusAsync(Guid id, UpdateSolicitacaoCompraStatusRequest request, Guid userId);
    Task<List<CotacaoResponse>> GetCotacoesAsync(Guid solicitacaoId, Guid userId);
    Task<CotacaoResponse> CreateCotacaoAsync(Guid solicitacaoId, CreateCotacaoRequest request, Guid userId);
    Task<CotacaoResponse> UpdateCotacaoAsync(Guid solicitacaoId, Guid cotacaoId, CreateCotacaoRequest request, Guid userId);
    Task SelecionarCotacaoAsync(Guid solicitacaoId, Guid cotacaoId, Guid userId);
    Task DeleteCotacaoAsync(Guid solicitacaoId, Guid cotacaoId, Guid userId);
}

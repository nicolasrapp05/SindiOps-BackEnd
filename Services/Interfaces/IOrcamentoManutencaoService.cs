using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;

namespace SindiOps.API.Services.Interfaces;

public interface IOrcamentoManutencaoService
{
    Task<List<OrcamentoManutencaoResponse>> GetAllAsync(Guid solicitacaoId, Guid userId);
    Task<OrcamentoManutencaoResponse> CreateAsync(Guid solicitacaoId, CreateOrcamentoManutencaoRequest request, Guid userId);
    Task<OrcamentoManutencaoResponse> UpdateAsync(Guid solicitacaoId, Guid orcamentoId, CreateOrcamentoManutencaoRequest request, Guid userId);
    Task SelecionarAsync(Guid solicitacaoId, Guid orcamentoId, Guid userId);
    Task DeleteAsync(Guid solicitacaoId, Guid orcamentoId, Guid userId);
    Task<SolicitacaoManutencaoResponse> AprovarAsync(Guid solicitacaoId, Guid userId);
    Task<SolicitacaoManutencaoResponse> ReabrirAsync(Guid solicitacaoId, Guid userId);
}

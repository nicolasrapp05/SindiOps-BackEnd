using Microsoft.AspNetCore.Http;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Helpers;

namespace SindiOps.API.Services.Interfaces;

public interface IOcorrenciaService
{
    Task<PaginatedResponse<OcorrenciaResponse>> GetAllAsync(Guid userId, OcorrenciaQueryParams queryParams);
    Task<OcorrenciaDetalheResponse> GetByIdAsync(Guid id, Guid userId);
    Task<OcorrenciaResponse> CreateAsync(CreateOcorrenciaRequest request, Guid userId);
    Task<OcorrenciaResponse> UpdateAsync(Guid id, CreateOcorrenciaRequest request, Guid userId);
    Task<OcorrenciaResponse> UpdateStatusAsync(Guid id, UpdateOcorrenciaStatusRequest request, Guid userId);
    Task<MidiaResponse> UploadMidiaAsync(Guid ocorrenciaId, IFormFile arquivo, string tipo, Guid userId);
    Task DeleteMidiaAsync(Guid ocorrenciaId, Guid midiaId, Guid userId);
}

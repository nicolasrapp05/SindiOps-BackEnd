using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;

namespace SindiOps.API.Services.Interfaces;

public interface IEmailLogService
{
    Task<EmailLogListaResponse> GetAllAsync(Guid userId, EmailLogQueryParams queryParams);
    Task<EmailLogDetalheResponse> GetByIdAsync(Guid id, Guid userId);
}

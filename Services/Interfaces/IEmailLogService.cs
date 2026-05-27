using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Helpers;

namespace SindiOps.API.Services.Interfaces;

public interface IEmailLogService
{
    Task<PaginatedResponse<EmailLogResponse>> GetAllAsync(Guid userId, EmailLogQueryParams queryParams);
    Task<EmailLogDetalheResponse> GetByIdAsync(Guid id, Guid userId);
}

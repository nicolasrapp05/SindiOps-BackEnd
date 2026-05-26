using SindiCore.API.DTOs.Requests;
using SindiCore.API.DTOs.Responses;
using SindiCore.API.Helpers;

namespace SindiCore.API.Services.Interfaces;

public interface IEmailLogService
{
    Task<PaginatedResponse<EmailLogResponse>> GetAllAsync(Guid userId, EmailLogQueryParams queryParams);
    Task<EmailLogDetalheResponse> GetByIdAsync(Guid id, Guid userId);
}

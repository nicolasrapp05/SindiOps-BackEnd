using SindiCore.API.DTOs.Requests;
using SindiCore.API.DTOs.Responses;

namespace SindiCore.API.Services.Interfaces;

public interface IEmailTemplateService
{
    Task<List<EmailTemplateResponse>> GetAllAsync(Guid userId, EmailTemplateQueryParams? queryParams);
    Task<EmailTemplateDetalheResponse> GetByIdAsync(Guid id, Guid userId);
    Task<EmailTemplateDetalheResponse> CreateAsync(CreateEmailTemplateRequest request, Guid userId);
    Task<EmailTemplateDetalheResponse> UpdateAsync(Guid id, CreateEmailTemplateRequest request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}

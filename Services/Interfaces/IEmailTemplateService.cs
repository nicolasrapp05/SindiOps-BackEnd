using SindiOps.API.DTOs.Requests;
using SindiOps.API.DTOs.Responses;

namespace SindiOps.API.Services.Interfaces;

public interface IEmailTemplateService
{
    Task<List<EmailTemplateResponse>> GetAllAsync(Guid userId, EmailTemplateQueryParams? queryParams);
    Task<EmailTemplateDetalheResponse> GetByIdAsync(Guid id, Guid userId);
    Task<EmailTemplateDetalheResponse> CreateAsync(CreateEmailTemplateRequest request, Guid userId);
    Task<EmailTemplateDetalheResponse> UpdateAsync(Guid id, CreateEmailTemplateRequest request, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
}

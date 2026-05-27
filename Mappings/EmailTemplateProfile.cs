using AutoMapper;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;

namespace SindiOps.API.Mappings;

public class EmailTemplateProfile : Profile
{
    public EmailTemplateProfile()
    {
        CreateMap<EmailTemplate, EmailTemplateResponse>();

        CreateMap<EmailTemplate, EmailTemplateDetalheResponse>()
            .IncludeBase<EmailTemplate, EmailTemplateResponse>();
    }
}

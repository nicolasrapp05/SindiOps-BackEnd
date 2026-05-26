using AutoMapper;
using SindiCore.API.DTOs.Responses;
using SindiCore.API.Entities;

namespace SindiCore.API.Mappings;

public class EmailTemplateProfile : Profile
{
    public EmailTemplateProfile()
    {
        CreateMap<EmailTemplate, EmailTemplateResponse>();

        CreateMap<EmailTemplate, EmailTemplateDetalheResponse>()
            .IncludeBase<EmailTemplate, EmailTemplateResponse>();
    }
}

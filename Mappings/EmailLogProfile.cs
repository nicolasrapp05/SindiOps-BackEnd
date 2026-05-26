using AutoMapper;
using SindiCore.API.DTOs.Responses;
using SindiCore.API.Entities;

namespace SindiCore.API.Mappings;

public class EmailLogProfile : Profile
{
    public EmailLogProfile()
    {
        CreateMap<Morador, MoradorEmailLogRefResponse>();

        CreateMap<Ocorrencia, OcorrenciaEmailLogRefResponse>();

        CreateMap<EmailTemplate, TemplateEmailLogRefResponse>();

        CreateMap<EmailLog, EmailLogResponse>()
            .ForMember(d => d.Morador, o => o.MapFrom(s => s.Morador))
            .ForMember(d => d.Ocorrencia, o => o.MapFrom(s => s.Ocorrencia))
            .ForMember(d => d.Template, o => o.MapFrom(s => s.Template))
            .ForMember(d => d.EnviadoPor, o => o.MapFrom(s => s.EnviadoPor));

        CreateMap<EmailLog, EmailLogDetalheResponse>()
            .IncludeBase<EmailLog, EmailLogResponse>();
    }
}

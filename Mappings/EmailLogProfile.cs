using AutoMapper;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;

namespace SindiOps.API.Mappings;

public class EmailLogProfile : Profile
{
    public EmailLogProfile()
    {
        CreateMap<Morador, MoradorEmailLogRefResponse>()
            .ForMember(d => d.Nome, o => o.MapFrom(s => s.Pessoa.Nome));

        CreateMap<Ocorrencia, OcorrenciaEmailLogRefResponse>();

        CreateMap<EmailTemplate, TemplateEmailLogRefResponse>();

        CreateMap<EmailLog, EmailLogResponse>()
            .ForMember(d => d.Morador, o => o.MapFrom(s => s.Morador))
            .ForMember(d => d.Ocorrencia, o => o.MapFrom(s => s.Ocorrencia))
            .ForMember(d => d.Template, o => o.MapFrom(s => s.Template))
            .ForMember(d => d.EnviadoPor, o => o.MapFrom(s => new PessoaRefResponse
            {
                Id = s.EnviadoPor.Id,
                Nome = s.EnviadoPor.Pessoa.Nome,
                Cargo = s.EnviadoPor.Cargo,
            }));

        CreateMap<EmailLog, EmailLogDetalheResponse>()
            .IncludeBase<EmailLog, EmailLogResponse>();
    }
}

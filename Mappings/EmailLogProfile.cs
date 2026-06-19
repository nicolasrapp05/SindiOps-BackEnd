using AutoMapper;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;

namespace SindiOps.API.Mappings;

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
            .ForMember(d => d.EnviadoPor, o => o.MapFrom(s =>
                s.EnviadoPorFuncionario != null
                    ? new PessoaRefResponse
                    {
                        Id = s.EnviadoPorFuncionario.Id,
                        Nome = s.EnviadoPorFuncionario.Nome,
                        Cargo = s.EnviadoPorFuncionario.Cargo,
                    }
                    : new PessoaRefResponse
                    {
                        Id = s.EnviadoPorSindico!.Id,
                        Nome = s.EnviadoPorSindico.Nome,
                    }));

        CreateMap<EmailLog, EmailLogDetalheResponse>()
            .IncludeBase<EmailLog, EmailLogResponse>();
    }
}

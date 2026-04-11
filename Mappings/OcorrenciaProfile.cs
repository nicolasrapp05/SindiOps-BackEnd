using AutoMapper;
using SindiCore.API.DTOs.Responses;
using SindiCore.API.Entities;

namespace SindiCore.API.Mappings;

public class OcorrenciaProfile : Profile
{
    private const int DescricaoListagemMax = 150;

    public OcorrenciaProfile()
    {
        CreateMap<Sindico, PessoaRefResponse>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Nome, o => o.MapFrom(s => s.Nome));

        CreateMap<Ocorrencia, OcorrenciaResponse>()
            .ForMember(d => d.Descricao, o => o.MapFrom(s =>
                s.Descricao.Length > DescricaoListagemMax
                    ? s.Descricao.Substring(0, DescricaoListagemMax)
                    : s.Descricao))
            .ForMember(d => d.Morador, o => o.MapFrom(s => s.Morador))
            .ForMember(d => d.Bloco, o => o.MapFrom(s => s.Bloco))
            .ForMember(d => d.Unidade, o => o.MapFrom(s => s.Unidade))
            .ForMember(d => d.RegistradoPor, o => o.MapFrom(s =>
                s.RegistradoPorFuncionario != null
                    ? new PessoaRefResponse
                    {
                        Id = s.RegistradoPorFuncionario.Id,
                        Nome = s.RegistradoPorFuncionario.Nome,
                    }
                    : new PessoaRefResponse
                    {
                        Id = s.RegistradoPorSindico!.Id,
                        Nome = s.RegistradoPorSindico.Nome,
                    }))
            .ForMember(d => d.TotalMidias, o => o.MapFrom(s => s.Midias.Count));

        CreateMap<Morador, MoradorOcorrenciaRefResponse>()
            .ForMember(d => d.Unidade, o => o.MapFrom(s => s.Unidade));

        CreateMap<Unidade, UnidadeNumeroRefResponse>();

        CreateMap<Bloco, BlocoNomeRefResponse>();

        CreateMap<Ocorrencia, OcorrenciaDetalheResponse>()
            .IncludeBase<Ocorrencia, OcorrenciaResponse>()
            .ForMember(d => d.Descricao, o => o.MapFrom(s => s.Descricao))
            .ForMember(d => d.Midias, o => o.Ignore())
            .ForMember(d => d.EmailLogs, o => o.MapFrom(s =>
                s.EmailLogs.OrderByDescending(e => e.EnviadoEm)));
    }
}

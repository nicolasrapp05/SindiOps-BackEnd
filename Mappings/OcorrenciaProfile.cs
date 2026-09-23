using AutoMapper;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;

namespace SindiOps.API.Mappings;

public class OcorrenciaProfile : Profile
{
    private const int DescricaoListagemMax = 150;

    public OcorrenciaProfile()
    {
        CreateMap<Ocorrencia, OcorrenciaResponse>()
            .ForMember(d => d.Descricao, o => o.MapFrom(s =>
                s.Descricao.Length > DescricaoListagemMax
                    ? s.Descricao.Substring(0, DescricaoListagemMax)
                    : s.Descricao))
            .ForMember(d => d.Morador, o => o.MapFrom(s => s.Morador))
            .ForMember(d => d.Bloco, o => o.MapFrom(s => s.Bloco))
            .ForMember(d => d.Unidade, o => o.MapFrom(s => s.Unidade))
            .ForMember(d => d.RegistradoPor, o => o.MapFrom(s => new PessoaRefResponse
            {
                Id = s.RegistradoPor.Id,
                Nome = s.RegistradoPor.Pessoa.Nome,
                Cargo = s.RegistradoPor.Cargo,
            }))
            .ForMember(d => d.TotalMidias, o => o.MapFrom(s => s.Midias.Count));

        CreateMap<Morador, MoradorOcorrenciaRefResponse>()
            .ForMember(d => d.Nome, o => o.MapFrom(s => s.Pessoa.Nome))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Pessoa.Email))
            .ForMember(d => d.Telefone, o => o.MapFrom(s => s.Pessoa.Telefone))
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

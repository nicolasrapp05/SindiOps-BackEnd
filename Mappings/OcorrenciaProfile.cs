using AutoMapper;
using SindiOps.API.Constants;
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
            .ForMember(d => d.Morador, o => o.MapFrom(s => MapMorador(s)))
            .ForMember(d => d.Bloco, o => o.MapFrom(s => s.Bloco))
            .ForMember(d => d.Unidade, o => o.MapFrom(s => s.Unidade))
            .ForMember(d => d.RegistradoPor, o => o.MapFrom(s => s.RegistradoPor))
            .ForMember(d => d.TotalMidias, o => o.MapFrom(s => s.Midias.Count));

        CreateMap<Usuario, PessoaRefResponse>()
            .ForMember(d => d.Nome, o => o.MapFrom(s => s.Nome))
            .ForMember(d => d.Cargo, o => o.MapFrom(s =>
                s.Sindico != null
                    ? CargoConstants.Sindico
                    : s.Funcionario != null ? s.Funcionario.Cargo : null));

        CreateMap<Unidade, UnidadeNumeroRefResponse>();

        CreateMap<Bloco, BlocoNomeRefResponse>();

        CreateMap<Ocorrencia, OcorrenciaDetalheResponse>()
            .IncludeBase<Ocorrencia, OcorrenciaResponse>()
            .ForMember(d => d.Descricao, o => o.MapFrom(s => s.Descricao))
            .ForMember(d => d.Midias, o => o.Ignore())
            .ForMember(d => d.EmailLogs, o => o.MapFrom(s =>
                s.EmailLogs.OrderByDescending(e => e.EnviadoEm)));
    }

    private static MoradorOcorrenciaRefResponse? MapMorador(Ocorrencia ocorrencia)
    {
        if (ocorrencia.Morador is null)
            return null;

        var ocupacao = ocorrencia.Morador.Unidades
                .FirstOrDefault(u => ocorrencia.UnidadeId != null && u.UnidadeId == ocorrencia.UnidadeId)
            ?? ocorrencia.Morador.Unidades.FirstOrDefault();

        if (ocupacao is null)
            return null;

        return new MoradorOcorrenciaRefResponse
        {
            Id = ocupacao.Id,
            Nome = ocorrencia.Morador.Usuario.Nome,
            Email = ocorrencia.Morador.Usuario.Email,
            Telefone = ocorrencia.Morador.Usuario.Telefone,
            Unidade = new UnidadeNumeroRefResponse
            {
                Numero = ocupacao.Unidade?.Numero ?? ocorrencia.Unidade?.Numero ?? string.Empty,
            },
        };
    }
}

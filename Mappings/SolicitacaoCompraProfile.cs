using AutoMapper;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;

namespace SindiOps.API.Mappings;

public class SolicitacaoCompraProfile : Profile
{
    public SolicitacaoCompraProfile()
    {
        CreateMap<Cotacao, CotacaoResponse>()
            .ForMember(d => d.Fornecedor, o => o.MapFrom(s => s.Fornecedor));

        CreateMap<SolicitacaoCompra, SolicitacaoCompraResponse>()
            .ForMember(d => d.SolicitadoPor, o => o.MapFrom(s =>
                s.SolicitadoPorFuncionario != null
                    ? new PessoaRefResponse
                    {
                        Id = s.SolicitadoPorFuncionario.Id,
                        Nome = s.SolicitadoPorFuncionario.Nome,
                    }
                    : new PessoaRefResponse
                    {
                        Id = s.SolicitadoPorSindico!.Id,
                        Nome = s.SolicitadoPorSindico.Nome,
                    }))
            .ForMember(d => d.AprovadoPor, o => o.MapFrom(s => s.AprovadoPor))
            .ForMember(d => d.TotalCotacoes, o => o.MapFrom(s => s.Cotacoes.Count))
            .ForMember(d => d.TemCotacaoSelecionada, o => o.MapFrom(s => s.Cotacoes.Any(c => c.Selecionada)));

        CreateMap<SolicitacaoCompra, SolicitacaoCompraDetalheResponse>()
            .IncludeBase<SolicitacaoCompra, SolicitacaoCompraResponse>()
            .ForMember(d => d.Cotacoes, o => o.MapFrom(s => s.Cotacoes));
    }
}

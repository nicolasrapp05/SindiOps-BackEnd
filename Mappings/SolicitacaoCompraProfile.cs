using AutoMapper;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;

namespace SindiOps.API.Mappings;

public class SolicitacaoCompraProfile : Profile
{
    public SolicitacaoCompraProfile()
    {
        CreateMap<Usuario, PessoaRefResponse>()
            .ForMember(d => d.Nome, o => o.MapFrom(s => s.Pessoa.Nome));

        CreateMap<SolicitacaoCompraItem, SolicitacaoCompraItemResponse>();
        CreateMap<CotacaoItem, CotacaoItemResponse>();

        CreateMap<Cotacao, CotacaoResponse>()
            .ForMember(d => d.Fornecedor, o => o.MapFrom(s => s.Fornecedor))
            .ForMember(d => d.ValorTotal, o => o.MapFrom(s => s.Itens.Sum(i => i.ValorTotal)))
            .ForMember(d => d.Itens, o => o.MapFrom(s => s.Itens));

        CreateMap<SolicitacaoCompra, SolicitacaoCompraResponse>()
            .ForMember(d => d.Itens, o => o.MapFrom(s => s.Itens.OrderBy(i => i.Ordem).ThenBy(i => i.Descricao)))
            .ForMember(d => d.SolicitadoPor, o => o.MapFrom(s => s.SolicitadoPor))
            .ForMember(d => d.AprovadoPor, o => o.MapFrom(s => s.AprovadoPor))
            .ForMember(d => d.TotalCotacoes, o => o.MapFrom(s => s.Cotacoes.Count))
            .ForMember(d => d.TemCotacaoSelecionada, o => o.MapFrom(s => s.Cotacoes.Any(c => c.Selecionada)));

        CreateMap<SolicitacaoCompra, SolicitacaoCompraDetalheResponse>()
            .IncludeBase<SolicitacaoCompra, SolicitacaoCompraResponse>()
            .ForMember(d => d.Cotacoes, o => o.MapFrom(s => s.Cotacoes.OrderBy(c => c.CriadoEm)));
    }
}

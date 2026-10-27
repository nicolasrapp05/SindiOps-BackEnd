using AutoMapper;
using SindiOps.API.Constants;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;

namespace SindiOps.API.Mappings;

public class SolicitacaoManutencaoProfile : Profile
{
    public SolicitacaoManutencaoProfile()
    {
        CreateMap<SolicitacaoManutencao, SolicitacaoManutencaoResponse>()
            .ForMember(d => d.TipoServico, o => o.MapFrom(s => s.TipoManutencao))
            .ForMember(d => d.RegistradoPor, o => o.MapFrom(s => s.SolicitadoPor))
            .ForMember(d => d.Fornecedor, o => o.MapFrom(s => s.Fornecedor))
            .ForMember(d => d.TotalOrcamentos, o => o.MapFrom(s => s.Orcamentos.Count))
            .ForMember(d => d.TemOrcamentoSelecionado, o => o.MapFrom(s => s.Orcamentos.Any(o => o.Selecionada)))
            .ForMember(d => d.OrcamentoAprovado, o => o.MapFrom(s => s.OrcamentoAprovadoPorId != null))
            .ForMember(d => d.SituacaoOrcamento, o => o.MapFrom(s => SituacaoOrcamento.Calcular(
                s.Status,
                s.Orcamentos.Count > 0,
                s.Orcamentos.Any(o => o.Selecionada),
                s.OrcamentoAprovadoPorId != null)))
            .ForMember(d => d.OrcamentoAprovadoPor, o => o.MapFrom(s => s.OrcamentoAprovadoPor));
    }
}

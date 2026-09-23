using AutoMapper;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;

namespace SindiOps.API.Mappings;

public class SolicitacaoManutencaoProfile : Profile
{
    public SolicitacaoManutencaoProfile()
    {
        CreateMap<SolicitacaoManutencao, SolicitacaoManutencaoResponse>()
            .ForMember(d => d.TipoServico, o => o.MapFrom(s => s.TipoManutencao))
            .ForMember(d => d.RegistradoPor, o => o.MapFrom(s => new PessoaRefResponse
            {
                Id = s.SolicitadoPor.Id,
                Nome = s.SolicitadoPor.Pessoa.Nome,
                Cargo = s.SolicitadoPor.Cargo,
            }))
            .ForMember(d => d.Fornecedor, o => o.MapFrom(s => s.Fornecedor));
    }
}

using AutoMapper;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;

namespace SindiOps.API.Mappings;

public class SolicitacaoManutencaoProfile : Profile
{
    public SolicitacaoManutencaoProfile()
    {
        CreateMap<Funcionario, PessoaRefResponse>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Nome, o => o.MapFrom(s => s.Nome));

        CreateMap<SolicitacaoManutencao, SolicitacaoManutencaoResponse>()
            .ForMember(d => d.TipoServico, o => o.MapFrom(s => s.Tipo))
            .ForMember(d => d.RegistradoPor, o => o.MapFrom(s =>
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
            .ForMember(d => d.Fornecedor, o => o.MapFrom(s => s.Fornecedor));
    }
}

using AutoMapper;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;

namespace SindiOps.API.Mappings;

public class FuncionarioProfile : Profile
{
    public FuncionarioProfile()
    {
        CreateMap<Funcionario, FuncionarioResponse>()
            .ForMember(d => d.Nome, o => o.MapFrom(s => s.Usuario.Nome))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Usuario.Email))
            .ForMember(d => d.Ativo, o => o.MapFrom(s => s.Usuario.Ativo))
            .ForMember(d => d.ConviteEnviado, o => o.Ignore())
            .ForMember(d => d.ConvitePendente, o => o.Ignore())
            .ForMember(d => d.Condominios, o => o.MapFrom(s =>
                s.CondominiosAcesso
                    .OrderBy(fc => fc.Condominio.Nome)
                    .Select(fc => new CondominioRefResponse
                    {
                        Id = fc.CondominioId,
                        Nome = fc.Condominio.Nome,
                    })));
    }
}

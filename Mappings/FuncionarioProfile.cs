using AutoMapper;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;

namespace SindiOps.API.Mappings;

public class FuncionarioProfile : Profile
{
    public FuncionarioProfile()
    {
        CreateMap<Usuario, FuncionarioResponse>()
            .ForMember(d => d.Nome, o => o.MapFrom(s => s.Pessoa.Nome))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.Pessoa.Email))
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

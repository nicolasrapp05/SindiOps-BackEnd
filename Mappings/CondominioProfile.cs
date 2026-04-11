using AutoMapper;
using SindiCore.API.DTOs.Responses;
using SindiCore.API.Entities;

namespace SindiCore.API.Mappings;

public class CondominioProfile : Profile
{
    public CondominioProfile()
    {
        CreateMap<Unidade, UnidadeResponse>();

        CreateMap<Bloco, BlocoResponse>();

        CreateMap<Condominio, CondominioResponse>()
            .ForMember(d => d.TotalBlocos, o => o.MapFrom(s => s.Blocos.Count))
            .ForMember(d => d.TotalUnidades, o => o.MapFrom(s => s.Unidades.Count));

        CreateMap<Condominio, CondominioDetalheResponse>()
            .IncludeBase<Condominio, CondominioResponse>();
    }
}

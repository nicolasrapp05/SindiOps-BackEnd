using AutoMapper;
using SindiCore.API.DTOs.Responses;
using SindiCore.API.Entities;

namespace SindiCore.API.Mappings;

public class ManutencaoObrigatoriaProfile : Profile
{
    public ManutencaoObrigatoriaProfile()
    {
        CreateMap<Condominio, CondominioRefResponse>();

        CreateMap<ManutencaoObrigatoria, ManutencaoObrigatoriaResponse>()
            .ForMember(d => d.Condominio, o => o.MapFrom(s => s.Condominio));
    }
}

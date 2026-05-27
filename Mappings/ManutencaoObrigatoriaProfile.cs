using AutoMapper;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;

namespace SindiOps.API.Mappings;

public class ManutencaoObrigatoriaProfile : Profile
{
    public ManutencaoObrigatoriaProfile()
    {
        CreateMap<Condominio, CondominioRefResponse>();

        CreateMap<ManutencaoObrigatoria, ManutencaoObrigatoriaResponse>()
            .ForMember(d => d.Condominio, o => o.MapFrom(s => s.Condominio));
    }
}

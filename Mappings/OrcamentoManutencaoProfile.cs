using AutoMapper;
using SindiOps.API.Constants;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;

namespace SindiOps.API.Mappings;

public class OrcamentoManutencaoProfile : Profile
{
    public OrcamentoManutencaoProfile()
    {
        CreateMap<OrcamentoManutencao, OrcamentoManutencaoResponse>()
            .ForMember(d => d.Fornecedor, o => o.MapFrom(s => s.Fornecedor));
    }
}

using AutoMapper;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;

namespace SindiOps.API.Mappings;

public class ContratoProfile : Profile
{
    public ContratoProfile()
    {
        CreateMap<Contrato, ContratoResponse>()
            .ForMember(d => d.Fornecedor, o => o.MapFrom(s => s.Fornecedor));

        CreateMap<Contrato, ContratoDetalheResponse>()
            .IncludeBase<Contrato, ContratoResponse>();
    }
}

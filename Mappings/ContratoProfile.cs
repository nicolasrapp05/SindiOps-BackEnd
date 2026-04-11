using AutoMapper;
using SindiCore.API.DTOs.Responses;
using SindiCore.API.Entities;

namespace SindiCore.API.Mappings;

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

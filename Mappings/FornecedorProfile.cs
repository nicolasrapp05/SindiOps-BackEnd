using AutoMapper;
using SindiCore.API.DTOs.Responses;
using SindiCore.API.Entities;

namespace SindiCore.API.Mappings;

public class FornecedorProfile : Profile
{
    public FornecedorProfile()
    {
        CreateMap<ServicoFornecedor, ServicoFornecedorResponse>();

        CreateMap<Fornecedor, FornecedorResponse>();

        CreateMap<Fornecedor, FornecedorDetalheResponse>()
            .IncludeBase<Fornecedor, FornecedorResponse>()
            .ForMember(d => d.Servicos, o => o.MapFrom(s => s.Servicos));

        CreateMap<Fornecedor, FornecedorRefResponse>();
    }
}

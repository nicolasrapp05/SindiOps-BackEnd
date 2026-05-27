using AutoMapper;
using SindiOps.API.DTOs.Responses;
using SindiOps.API.Entities;

namespace SindiOps.API.Mappings;

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

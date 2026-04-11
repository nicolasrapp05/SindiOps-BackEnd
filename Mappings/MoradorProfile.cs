using AutoMapper;
using SindiCore.API.DTOs.Responses;
using SindiCore.API.Entities;

namespace SindiCore.API.Mappings;

public class MoradorProfile : Profile
{
    public MoradorProfile()
    {
        CreateMap<Bloco, BlocoRefResponse>();

        CreateMap<Morador, MoradorResponse>()
            .ForMember(d => d.Bloco, o => o.MapFrom(s => s.Bloco))
            .ForMember(d => d.Unidade, o => o.MapFrom(s => s.Unidade));

        CreateMap<Morador, MoradorDetalheResponse>()
            .IncludeBase<Morador, MoradorResponse>()
            .ForMember(d => d.UltimosEmails, o => o.MapFrom(s =>
                s.EmailLogs.OrderByDescending(e => e.EnviadoEm).Take(5)));

        CreateMap<EmailLog, EmailLogResumoResponse>();
    }
}

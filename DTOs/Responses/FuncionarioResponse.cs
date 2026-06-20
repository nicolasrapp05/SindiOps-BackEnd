namespace SindiOps.API.DTOs.Responses;

public class FuncionarioResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime CriadoEm { get; set; }
    public bool? ConviteEnviado { get; set; }
    public bool? ConvitePendente { get; set; }
    public List<CondominioRefResponse> Condominios { get; set; } = [];
}

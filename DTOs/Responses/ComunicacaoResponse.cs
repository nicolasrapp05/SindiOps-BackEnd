namespace SindiCore.API.DTOs.Responses;

public class ComunicacaoResponse
{
    public Guid Id { get; set; }
    public string EmailDestinatario { get; set; } = string.Empty;
    public string Assunto { get; set; } = string.Empty;
    public string StatusEntrega { get; set; } = string.Empty;
    public DateTime EnviadoEm { get; set; }
}

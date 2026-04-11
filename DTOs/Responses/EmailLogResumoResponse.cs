namespace SindiCore.API.DTOs.Responses;

public class EmailLogResumoResponse
{
    public Guid Id { get; set; }
    public string Assunto { get; set; } = string.Empty;
    public DateTime EnviadoEm { get; set; }
    public string StatusEntrega { get; set; } = string.Empty;
}

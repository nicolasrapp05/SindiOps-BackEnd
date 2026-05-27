namespace SindiOps.API.DTOs.Responses;

public class MidiaResponse
{
    public Guid Id { get; set; }
    public string SignedUrl { get; set; } = string.Empty;
    public string TipoArquivo { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
}

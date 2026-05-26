namespace SindiCore.API.DTOs.Responses;

public class TemplateEmailLogRefResponse
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
}

namespace SindiOps.API.DTOs.Requests;

public class CreateEmailTemplateRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Assunto { get; set; } = string.Empty;
    public string Corpo { get; set; } = string.Empty;
}

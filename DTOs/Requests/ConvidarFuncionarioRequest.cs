namespace SindiCore.API.DTOs.Requests;

public class ConvidarFuncionarioRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
}

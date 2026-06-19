namespace SindiOps.API.DTOs.Requests;

public class CadastroSindicoRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
    public string ConfirmarSenha { get; set; } = string.Empty;
}

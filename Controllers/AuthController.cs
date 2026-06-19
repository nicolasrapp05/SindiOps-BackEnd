using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.Helpers;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/v1/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _service;

    public AuthController(IAuthService service)
    {
        _service = service;
    }

    [HttpPost("cadastro-sindico")]
    public async Task<IActionResult> CadastroSindico([FromBody] CadastroSindicoRequest request)
    {
        var data = await _service.CadastroSindicoAsync(request);
        return Ok(ApiResponse<object>.Ok(data, "Conta criada com sucesso"));
    }

    [HttpPost("esqueci-senha")]
    public async Task<IActionResult> EsqueciSenha([FromBody] EsqueciSenhaRequest request)
    {
        await _service.EsqueciSenhaAsync(request);
        return Ok(ApiResponse<object>.Ok(null, "Se existir uma conta com este email, você receberá instruções em breve."));
    }
}

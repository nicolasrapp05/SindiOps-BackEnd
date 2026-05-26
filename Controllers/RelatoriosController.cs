using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SindiCore.API.DTOs.Requests;
using SindiCore.API.Helpers;
using SindiCore.API.Services.Interfaces;

namespace SindiCore.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/relatorios")]
public class RelatoriosController : ControllerBase
{
    private readonly IRelatorioService _service;

    public RelatoriosController(IRelatorioService service)
    {
        _service = service;
    }

    [HttpPost("gerar")]
    public async Task<IActionResult> Gerar([FromBody] GerarRelatorioRequest request)
    {
        var userId = User.GetUserId();
        var (conteudo, contentType, fileName) = await _service.GerarRelatorioAsync(request, userId);
        return File(conteudo, contentType, fileName);
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SindiOps.API.Authorization;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.Helpers;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Controllers;

[Authorize]
[RequireAdminCargo]
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

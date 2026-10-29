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
    private readonly ICurrentUserService _currentUser;

    public RelatoriosController(IRelatorioService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpPost("gerar")]
    public async Task<IActionResult> Gerar([FromBody] GerarRelatorioRequest request)
    {
        var userId = User.GetUserId();
        var cargo = await _currentUser.GetCargoAsync();
        var (conteudo, contentType, fileName) = await _service.GerarRelatorioAsync(request, userId, cargo);
        return File(conteudo, contentType, fileName);
    }
}

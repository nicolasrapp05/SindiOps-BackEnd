using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.Helpers;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/manutencoes-obrigatorias")]
public class ManutencoesObrigatoriasController : ControllerBase
{
    private readonly IManutencaoObrigatoriaService _service;

    public ManutencoesObrigatoriasController(IManutencaoObrigatoriaService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ManutencaoObrigatoriaQueryParams queryParams)
    {
        var userId = User.GetUserId();
        var data = await _service.GetAllAsync(userId, queryParams);
        return Ok(data);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = User.GetUserId();
        var data = await _service.GetByIdAsync(id, userId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateManutencaoObrigatoriaRequest request)
    {
        var userId = User.GetUserId();
        var data = await _service.CreateAsync(request, userId);
        return StatusCode(201, ApiResponse<object>.Ok(data, "Manutenção obrigatória criada com sucesso"));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateManutencaoObrigatoriaRequest request)
    {
        var userId = User.GetUserId();
        var data = await _service.UpdateAsync(id, request, userId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpPatch("{id:guid}/realizar")]
    public async Task<IActionResult> Realizar(Guid id, [FromBody] RealizarManutencaoRequest request)
    {
        var userId = User.GetUserId();
        var data = await _service.RealizarAsync(id, request, userId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.GetUserId();
        await _service.DeleteAsync(id, userId);
        return Ok(ApiResponse<object?>.Ok(null, "Manutenção obrigatória removida com sucesso"));
    }
}

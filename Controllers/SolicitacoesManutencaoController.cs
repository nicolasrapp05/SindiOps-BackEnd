using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.Helpers;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/solicitacoes-manutencao")]
public class SolicitacoesManutencaoController : ControllerBase
{
    private readonly ISolicitacaoManutencaoService _service;

    public SolicitacoesManutencaoController(ISolicitacaoManutencaoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SolicitacaoManutencaoQueryParams queryParams)
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
    public async Task<IActionResult> Create([FromBody] CreateSolicitacaoManutencaoRequest request)
    {
        var userId = User.GetUserId();
        var data = await _service.CreateAsync(request, userId);
        return StatusCode(201, ApiResponse<object>.Ok(data, "Solicitação criada com sucesso"));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateSolicitacaoManutencaoRequest request)
    {
        var userId = User.GetUserId();
        var data = await _service.UpdateAsync(id, request, userId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateSolicitacaoStatusRequest request)
    {
        var userId = User.GetUserId();
        var data = await _service.UpdateStatusAsync(id, request, userId);
        return Ok(ApiResponse<object>.Ok(data, "Status atualizado com sucesso"));
    }
}

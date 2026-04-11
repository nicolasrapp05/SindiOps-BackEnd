using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SindiCore.API.DTOs.Requests;
using SindiCore.API.Helpers;
using SindiCore.API.Services.Interfaces;

namespace SindiCore.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/solicitacoes-compra")]
public class SolicitacoesCompraController : ControllerBase
{
    private readonly ISolicitacaoCompraService _service;

    public SolicitacoesCompraController(ISolicitacaoCompraService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] SolicitacaoCompraQueryParams queryParams)
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
    public async Task<IActionResult> Create([FromBody] CreateSolicitacaoCompraRequest request)
    {
        var userId = User.GetUserId();
        var data = await _service.CreateAsync(request, userId);
        return StatusCode(201, ApiResponse<object>.Ok(data, "Solicitação de compra criada com sucesso"));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateSolicitacaoCompraRequest request)
    {
        var userId = User.GetUserId();
        var data = await _service.UpdateAsync(id, request, userId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpPatch("{id:guid}/aprovar")]
    public async Task<IActionResult> Aprovar(Guid id)
    {
        var userId = User.GetUserId();
        try
        {
            var data = await _service.AprovarAsync(id, userId);
            return Ok(ApiResponse<object>.Ok(data, "Solicitação aprovada com sucesso"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateSolicitacaoCompraStatusRequest request)
    {
        var userId = User.GetUserId();
        var data = await _service.UpdateStatusAsync(id, request, userId);
        return Ok(ApiResponse<object>.Ok(data, "Status atualizado com sucesso"));
    }

    [HttpGet("{id:guid}/cotacoes")]
    public async Task<IActionResult> GetCotacoes(Guid id)
    {
        var userId = User.GetUserId();
        var data = await _service.GetCotacoesAsync(id, userId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpPost("{id:guid}/cotacoes")]
    public async Task<IActionResult> CreateCotacao(Guid id, [FromBody] CreateCotacaoRequest request)
    {
        var userId = User.GetUserId();
        var data = await _service.CreateCotacaoAsync(id, request, userId);
        return StatusCode(201, ApiResponse<object>.Ok(data, "Cotação criada com sucesso"));
    }

    [HttpPut("{solicitacaoId:guid}/cotacoes/{cotacaoId:guid}")]
    public async Task<IActionResult> UpdateCotacao(
        Guid solicitacaoId, Guid cotacaoId, [FromBody] CreateCotacaoRequest request)
    {
        var userId = User.GetUserId();
        var data = await _service.UpdateCotacaoAsync(solicitacaoId, cotacaoId, request, userId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpPatch("{solicitacaoId:guid}/cotacoes/{cotacaoId:guid}/selecionar")]
    public async Task<IActionResult> SelecionarCotacao(Guid solicitacaoId, Guid cotacaoId)
    {
        var userId = User.GetUserId();
        await _service.SelecionarCotacaoAsync(solicitacaoId, cotacaoId, userId);
        return Ok(ApiResponse<object?>.Ok(null, "Cotação selecionada com sucesso"));
    }

    [HttpDelete("{solicitacaoId:guid}/cotacoes/{cotacaoId:guid}")]
    public async Task<IActionResult> DeleteCotacao(Guid solicitacaoId, Guid cotacaoId)
    {
        var userId = User.GetUserId();
        await _service.DeleteCotacaoAsync(solicitacaoId, cotacaoId, userId);
        return Ok(ApiResponse<object?>.Ok(null, "Cotação removida com sucesso"));
    }
}

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
[Route("api/v1/solicitacoes-manutencao/{solicitacaoId:guid}/orcamentos")]
public class OrcamentosManutencaoController : ControllerBase
{
    private readonly IOrcamentoManutencaoService _service;

    public OrcamentosManutencaoController(IOrcamentoManutencaoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid solicitacaoId)
    {
        var data = await _service.GetAllAsync(solicitacaoId, User.GetUserId());
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid solicitacaoId, [FromBody] CreateOrcamentoManutencaoRequest request)
    {
        var data = await _service.CreateAsync(solicitacaoId, request, User.GetUserId());
        return StatusCode(201, ApiResponse<object>.Ok(data, "Orçamento criado com sucesso"));
    }

    [HttpPut("{orcamentoId:guid}")]
    public async Task<IActionResult> Update(
        Guid solicitacaoId, Guid orcamentoId, [FromBody] CreateOrcamentoManutencaoRequest request)
    {
        var data = await _service.UpdateAsync(solicitacaoId, orcamentoId, request, User.GetUserId());
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpPatch("{orcamentoId:guid}/selecionar")]
    public async Task<IActionResult> Selecionar(Guid solicitacaoId, Guid orcamentoId)
    {
        await _service.SelecionarAsync(solicitacaoId, orcamentoId, User.GetUserId());
        return Ok(ApiResponse<object?>.Ok(null, "Orçamento selecionado com sucesso"));
    }

    [HttpDelete("{orcamentoId:guid}")]
    public async Task<IActionResult> Delete(Guid solicitacaoId, Guid orcamentoId)
    {
        await _service.DeleteAsync(solicitacaoId, orcamentoId, User.GetUserId());
        return Ok(ApiResponse<object?>.Ok(null, "Orçamento removido com sucesso"));
    }

    [HttpPatch("aprovar")]
    [RequireSindicoCargo]
    public async Task<IActionResult> Aprovar(Guid solicitacaoId)
    {
        try
        {
            var data = await _service.AprovarAsync(solicitacaoId, User.GetUserId());
            return Ok(ApiResponse<object>.Ok(data, "Orçamento aprovado com sucesso"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.Fail(ex.Message));
        }
    }

    [HttpPatch("reabrir")]
    [RequireSindicoCargo]
    public async Task<IActionResult> Reabrir(Guid solicitacaoId)
    {
        try
        {
            var data = await _service.ReabrirAsync(solicitacaoId, User.GetUserId());
            return Ok(ApiResponse<object>.Ok(data, "Orçamento reaberto com sucesso"));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<object>.Fail(ex.Message));
        }
    }
}

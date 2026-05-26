using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SindiCore.API.DTOs.Requests;
using SindiCore.API.Helpers;
using SindiCore.API.Services.Interfaces;

namespace SindiCore.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/ocorrencias")]
public class OcorrenciasController : ControllerBase
{
    private readonly IOcorrenciaService _service;
    private readonly IComunicacaoService _comunicacaoService;

    public OcorrenciasController(IOcorrenciaService service, IComunicacaoService comunicacaoService)
    {
        _service = service;
        _comunicacaoService = comunicacaoService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] OcorrenciaQueryParams queryParams)
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
    public async Task<IActionResult> Create([FromBody] CreateOcorrenciaRequest request)
    {
        var userId = User.GetUserId();
        var data = await _service.CreateAsync(request, userId);
        return StatusCode(201, ApiResponse<object>.Ok(data, "Ocorrência registrada com sucesso"));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateOcorrenciaRequest request)
    {
        var userId = User.GetUserId();
        var data = await _service.UpdateAsync(id, request, userId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOcorrenciaStatusRequest request)
    {
        var userId = User.GetUserId();
        var data = await _service.UpdateStatusAsync(id, request, userId);
        return Ok(ApiResponse<object>.Ok(data, "Status atualizado com sucesso"));
    }

    [HttpPost("{id:guid}/midias")]
    [RequestSizeLimit(52_428_800)]
    public async Task<IActionResult> UploadMidia(
        Guid id,
        IFormFile arquivo,
        [FromForm] string tipo)
    {
        var userId = User.GetUserId();
        var data = await _service.UploadMidiaAsync(id, arquivo, tipo, userId);
        return StatusCode(201, ApiResponse<object>.Ok(data, "Mídia enviada com sucesso"));
    }

    [HttpDelete("{id:guid}/midias/{midiaId:guid}")]
    public async Task<IActionResult> DeleteMidia(Guid id, Guid midiaId)
    {
        var userId = User.GetUserId();
        await _service.DeleteMidiaAsync(id, midiaId, userId);
        return Ok(ApiResponse<object?>.Ok(null, "Mídia removida com sucesso"));
    }

    [HttpPost("{id:guid}/comunicacoes")]
    public async Task<IActionResult> EnviarComunicacao(Guid id, [FromBody] EnviarComunicacaoRequest request)
    {
        var userId = User.GetUserId();
        var data = await _comunicacaoService.EnviarComunicacaoAsync(id, request, userId);
        return StatusCode(201, ApiResponse<object>.Ok(data, "Comunicação processada"));
    }
}

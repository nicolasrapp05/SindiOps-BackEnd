using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SindiCore.API.DTOs.Requests;
using SindiCore.API.Helpers;
using SindiCore.API.Services.Interfaces;

namespace SindiCore.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/moradores")]
public class MoradoresController : ControllerBase
{
    private readonly IMoradorService _service;

    public MoradoresController(IMoradorService service)
    {
        _service = service;
    }

    // GET api/v1/moradores?condominioId=&blocoId=&unidadeId=&search=&page=&pageSize=
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid condominioId,
        [FromQuery] MoradorQueryParams queryParams)
    {
        var sindicoId = User.GetUserId();
        var data = await _service.GetAllAsync(condominioId, sindicoId, queryParams);
        return Ok(data);
    }

    // GET api/v1/moradores/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var sindicoId = User.GetUserId();
        var data = await _service.GetByIdAsync(id, sindicoId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    // POST api/v1/moradores
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMoradorRequest request)
    {
        var sindicoId = User.GetUserId();
        var data = await _service.CreateAsync(request, sindicoId);
        return StatusCode(201, ApiResponse<object>.Ok(data, "Morador cadastrado com sucesso"));
    }

    // PUT api/v1/moradores/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMoradorRequest request)
    {
        var sindicoId = User.GetUserId();
        var data = await _service.UpdateAsync(id, request, sindicoId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    // DELETE api/v1/moradores/{id}  — soft delete
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var sindicoId = User.GetUserId();
        await _service.DeleteAsync(id, sindicoId);
        return Ok(ApiResponse<object?>.Ok(null, "Morador removido com sucesso"));
    }

    // GET api/v1/moradores/{id}/email-logs
    [HttpGet("{id:guid}/email-logs")]
    public async Task<IActionResult> GetEmailLogs(
        Guid id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var sindicoId = User.GetUserId();
        var data = await _service.GetEmailLogsAsync(id, sindicoId, page, pageSize);
        return Ok(data);
    }
}

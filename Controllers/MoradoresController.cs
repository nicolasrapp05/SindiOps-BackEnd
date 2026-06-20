using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SindiOps.API.Authorization;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.Helpers;
using SindiOps.API.Infrastructure.Data;
using SindiOps.API.Services;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Controllers;

[Authorize]
[RequireAdminCargo]
[ApiController]
[Route("api/v1/moradores")]
public class MoradoresController : ControllerBase
{
    private readonly IMoradorService _service;
    private readonly SindiOpsDbContext _db;

    public MoradoresController(IMoradorService service, SindiOpsDbContext db)
    {
        _service = service;
        _db = db;
    }

    private async Task<Guid> GetSindicoScopeIdAsync()
    {
        var userId = User.GetUserId();
        return await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);
    }

    // GET api/v1/moradores?condominioId=&blocoId=&unidadeId=&search=&page=&pageSize=
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid condominioId,
        [FromQuery] MoradorQueryParams queryParams)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.GetAllAsync(condominioId, sindicoId, queryParams);
        return Ok(data);
    }

    // GET api/v1/moradores/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.GetByIdAsync(id, sindicoId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    // POST api/v1/moradores
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateMoradorRequest request)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.CreateAsync(request, sindicoId);
        return StatusCode(201, ApiResponse<object>.Ok(data, "Morador cadastrado com sucesso"));
    }

    // PUT api/v1/moradores/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateMoradorRequest request)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.UpdateAsync(id, request, sindicoId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    // DELETE api/v1/moradores/{id}  — soft delete
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
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
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.GetEmailLogsAsync(id, sindicoId, page, pageSize);
        return Ok(data);
    }
}

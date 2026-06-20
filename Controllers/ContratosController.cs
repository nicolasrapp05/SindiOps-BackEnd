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
[Route("api/v1/contratos")]
public class ContratosController : ControllerBase
{
    private readonly IContratoService _service;
    private readonly SindiOpsDbContext _db;

    public ContratosController(IContratoService service, SindiOpsDbContext db)
    {
        _service = service;
        _db = db;
    }

    private async Task<Guid> GetSindicoScopeIdAsync()
    {
        var userId = User.GetUserId();
        return await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);
    }

    // GET api/v1/contratos?condominioId=&status=&page=&pageSize=
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ContratoQueryParams queryParams)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.GetAllAsync(sindicoId, queryParams);
        return Ok(data);
    }

    // GET api/v1/contratos/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.GetByIdAsync(id, sindicoId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    // POST api/v1/contratos
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContratoRequest request)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.CreateAsync(request, sindicoId);
        return StatusCode(201, ApiResponse<object>.Ok(data, "Contrato criado com sucesso"));
    }

    // PUT api/v1/contratos/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateContratoRequest request)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.UpdateAsync(id, request, sindicoId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    // PATCH api/v1/contratos/{id}/status  — cancelar contrato
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateContratoStatusRequest request)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.UpdateStatusAsync(id, request, sindicoId);
        return Ok(ApiResponse<object>.Ok(data, "Contrato cancelado com sucesso"));
    }

    // PATCH api/v1/contratos/{id}/reativar
    [HttpPatch("{id:guid}/reativar")]
    public async Task<IActionResult> Reativar(Guid id)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.ReativarAsync(id, sindicoId);
        return Ok(ApiResponse<object>.Ok(data, "Contrato reativado com sucesso"));
    }
}

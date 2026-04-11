using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SindiCore.API.DTOs.Requests;
using SindiCore.API.Helpers;
using SindiCore.API.Services.Interfaces;

namespace SindiCore.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/contratos")]
public class ContratosController : ControllerBase
{
    private readonly IContratoService _service;

    public ContratosController(IContratoService service)
    {
        _service = service;
    }

    // GET api/v1/contratos?condominioId=&status=&page=&pageSize=
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ContratoQueryParams queryParams)
    {
        var sindicoId = User.GetUserId();
        var data = await _service.GetAllAsync(sindicoId, queryParams);
        return Ok(data);
    }

    // GET api/v1/contratos/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var sindicoId = User.GetUserId();
        var data = await _service.GetByIdAsync(id, sindicoId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    // POST api/v1/contratos
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateContratoRequest request)
    {
        var sindicoId = User.GetUserId();
        var data = await _service.CreateAsync(request, sindicoId);
        return StatusCode(201, ApiResponse<object>.Ok(data, "Contrato criado com sucesso"));
    }

    // PUT api/v1/contratos/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateContratoRequest request)
    {
        var sindicoId = User.GetUserId();
        var data = await _service.UpdateAsync(id, request, sindicoId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    // PATCH api/v1/contratos/{id}/status
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateContratoStatusRequest request)
    {
        var sindicoId = User.GetUserId();
        var data = await _service.UpdateStatusAsync(id, request, sindicoId);
        return Ok(ApiResponse<object>.Ok(data, "Status atualizado com sucesso"));
    }
}

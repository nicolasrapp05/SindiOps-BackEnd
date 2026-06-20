using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SindiOps.API.Authorization;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.Helpers;
using SindiOps.API.Infrastructure.Data;
using SindiOps.API.Services;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Controllers;

[Authorize]
[RequireSindicoCargo]
[ApiController]
[Route("api/v1/funcionarios")]
public class FuncionariosController : ControllerBase
{
    private readonly IFuncionarioService _service;
    private readonly SindiOpsDbContext _db;

    public FuncionariosController(IFuncionarioService service, SindiOpsDbContext db)
    {
        _service = service;
        _db = db;
    }

    private async Task<Guid> GetSindicoScopeIdAsync()
    {
        var userId = User.GetUserId();
        return await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);
    }

    // GET api/v1/funcionarios?cargo=&ativo=
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? cargo,
        [FromQuery] bool? ativo)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.GetAllAsync(sindicoId, cargo, ativo);
        return Ok(ApiResponse<object>.Ok(data));
    }

    // GET api/v1/funcionarios/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.GetByIdAsync(id, sindicoId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    // POST api/v1/funcionarios/convidar
    [HttpPost("convidar")]
    public async Task<IActionResult> Convidar([FromBody] ConvidarFuncionarioRequest request)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.ConvidarAsync(request, sindicoId);
        return StatusCode(201, ApiResponse<object>.Ok(data, "Funcionário convidado com sucesso"));
    }

    // POST api/v1/funcionarios/{id}/reenviar-convite
    [HttpPost("{id:guid}/reenviar-convite")]
    public async Task<IActionResult> ReenviarConvite(Guid id)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.ReenviarConviteAsync(id, sindicoId);
        return Ok(ApiResponse<object>.Ok(data, "Convite reenviado com sucesso"));
    }

    // PUT api/v1/funcionarios/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFuncionarioRequest request)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.UpdateAsync(id, request, sindicoId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    // PATCH api/v1/funcionarios/{id}/ativar
    [HttpPatch("{id:guid}/ativar")]
    public async Task<IActionResult> Ativar(Guid id)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.AtivarAsync(id, sindicoId);
        return Ok(ApiResponse<object>.Ok(data, "Funcionário ativado com sucesso"));
    }

    // PATCH api/v1/funcionarios/{id}/desativar
    [HttpPatch("{id:guid}/desativar")]
    public async Task<IActionResult> Desativar(Guid id)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var currentUserId = User.GetUserId();
        try
        {
            var data = await _service.DesativarAsync(id, sindicoId, currentUserId);
            return Ok(ApiResponse<object>.Ok(data, "Funcionário desativado com sucesso"));
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(403, ApiResponse<object>.Fail(ex.Message));
        }
    }
}

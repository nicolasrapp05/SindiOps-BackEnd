using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.Helpers;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/funcionarios")]
public class FuncionariosController : ControllerBase
{
    private readonly IFuncionarioService _service;

    public FuncionariosController(IFuncionarioService service)
    {
        _service = service;
    }

    // GET api/v1/funcionarios?cargo=&ativo=
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? cargo,
        [FromQuery] bool? ativo)
    {
        var sindicoId = User.GetUserId();
        var data = await _service.GetAllAsync(sindicoId, cargo, ativo);
        return Ok(ApiResponse<object>.Ok(data));
    }

    // GET api/v1/funcionarios/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var sindicoId = User.GetUserId();
        var data = await _service.GetByIdAsync(id, sindicoId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    // POST api/v1/funcionarios/convidar
    [HttpPost("convidar")]
    public async Task<IActionResult> Convidar([FromBody] ConvidarFuncionarioRequest request)
    {
        var sindicoId = User.GetUserId();
        var data = await _service.ConvidarAsync(request, sindicoId);
        return StatusCode(201, ApiResponse<object>.Ok(data, "Funcionário convidado com sucesso"));
    }

    // PUT api/v1/funcionarios/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateFuncionarioRequest request)
    {
        var sindicoId = User.GetUserId();
        var data = await _service.UpdateAsync(id, request, sindicoId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    // PATCH api/v1/funcionarios/{id}/ativar
    [HttpPatch("{id:guid}/ativar")]
    public async Task<IActionResult> Ativar(Guid id)
    {
        var sindicoId = User.GetUserId();
        var data = await _service.AtivarAsync(id, sindicoId);
        return Ok(ApiResponse<object>.Ok(data, "Funcionário ativado com sucesso"));
    }

    // PATCH api/v1/funcionarios/{id}/desativar
    [HttpPatch("{id:guid}/desativar")]
    public async Task<IActionResult> Desativar(Guid id)
    {
        var sindicoId = User.GetUserId();
        try
        {
            var data = await _service.DesativarAsync(id, sindicoId, sindicoId);
            return Ok(ApiResponse<object>.Ok(data, "Funcionário desativado com sucesso"));
        }
        catch (InvalidOperationException ex)
        {
            return StatusCode(403, ApiResponse<object>.Fail(ex.Message));
        }
    }
}

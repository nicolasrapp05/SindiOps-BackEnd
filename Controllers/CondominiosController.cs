using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.Helpers;
using SindiOps.API.Infrastructure.Data;
using SindiOps.API.Services;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/condominios")]
public class CondominiosController : ControllerBase
{
    private readonly ICondominioService _service;
    private readonly SindiOpsDbContext _db;

    public CondominiosController(ICondominioService service, SindiOpsDbContext db)
    {
        _service = service;
        _db = db;
    }

    private async Task<Guid> GetSindicoScopeIdAsync()
    {
        var userId = User.GetUserId();
        return await UsuarioSindicoScope.ResolveSindicoIdAsync(_db, userId);
    }

    // GET api/v1/condominios
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.GetAllAsync(sindicoId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    // GET api/v1/condominios/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.GetByIdAsync(id, sindicoId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    // POST api/v1/condominios
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateCondominioRequest request)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.CreateAsync(request, sindicoId);
        return StatusCode(201, ApiResponse<object>.Ok(data, "Condomínio criado com sucesso"));
    }

    // PUT api/v1/condominios/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateCondominioRequest request)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.UpdateAsync(id, request, sindicoId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    // DELETE api/v1/condominios/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        try
        {
            await _service.DeleteAsync(id, sindicoId);
            return Ok(ApiResponse<object?>.Ok(null, "Condomínio removido com sucesso"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // GET api/v1/condominios/{id}/blocos
    [HttpGet("{id:guid}/blocos")]
    public async Task<IActionResult> GetBlocos(Guid id)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.GetBlocosAsync(id, sindicoId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    // POST api/v1/condominios/{id}/blocos
    [HttpPost("{id:guid}/blocos")]
    public async Task<IActionResult> CreateBloco(Guid id, [FromBody] CreateBlocoRequest request)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.CreateBlocoAsync(id, request, sindicoId);
        return StatusCode(201, ApiResponse<object>.Ok(data, "Bloco criado com sucesso"));
    }

    // POST api/v1/condominios/{condominioId}/blocos/{blocoId}/unidades
    [HttpPost("{condominioId:guid}/blocos/{blocoId:guid}/unidades")]
    public async Task<IActionResult> CreateUnidade(
        Guid condominioId, Guid blocoId, [FromBody] CreateUnidadeRequest request)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.CreateUnidadeAsync(condominioId, blocoId, request, sindicoId);
        return StatusCode(201, ApiResponse<object>.Ok(data, "Unidade criada com sucesso"));
    }

    // DELETE api/v1/condominios/{condominioId}/blocos/{blocoId}
    [HttpDelete("{condominioId:guid}/blocos/{blocoId:guid}")]
    public async Task<IActionResult> DeleteBloco(Guid condominioId, Guid blocoId)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        try
        {
            await _service.DeleteBlocoAsync(condominioId, blocoId, sindicoId);
            return Ok(ApiResponse<object?>.Ok(null, "Bloco removido com sucesso"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }

    // PUT api/v1/condominios/{condominioId}/blocos/{blocoId}
    [HttpPut("{condominioId:guid}/blocos/{blocoId:guid}")]
    public async Task<IActionResult> UpdateBloco(
        Guid condominioId, Guid blocoId, [FromBody] UpdateBlocoRequest request)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.UpdateBlocoAsync(condominioId, blocoId, request, sindicoId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    // PUT api/v1/condominios/{condominioId}/blocos/{blocoId}/unidades/{unidadeId}
    [HttpPut("{condominioId:guid}/blocos/{blocoId:guid}/unidades/{unidadeId:guid}")]
    public async Task<IActionResult> UpdateUnidade(
        Guid condominioId, Guid blocoId, Guid unidadeId, [FromBody] UpdateUnidadeRequest request)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        var data = await _service.UpdateUnidadeAsync(condominioId, blocoId, unidadeId, request, sindicoId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    // DELETE api/v1/condominios/{condominioId}/blocos/{blocoId}/unidades/{unidadeId}
    [HttpDelete("{condominioId:guid}/blocos/{blocoId:guid}/unidades/{unidadeId:guid}")]
    public async Task<IActionResult> DeleteUnidade(Guid condominioId, Guid blocoId, Guid unidadeId)
    {
        var sindicoId = await GetSindicoScopeIdAsync();
        try
        {
            await _service.DeleteUnidadeAsync(condominioId, blocoId, unidadeId, sindicoId);
            return Ok(ApiResponse<object?>.Ok(null, "Unidade removida com sucesso"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }
}

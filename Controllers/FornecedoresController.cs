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
[Route("api/v1/fornecedores")]
public class FornecedoresController : ControllerBase
{
    private readonly IFornecedorService _service;
    private readonly ICurrentUserService _currentUser;

    public FornecedoresController(IFornecedorService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    // GET api/v1/fornecedores?search=&page=&pageSize=
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] FornecedorQueryParams queryParams)
    {
        var sindicoId = await _currentUser.GetSindicoScopeIdAsync();
        var data = await _service.GetAllAsync(sindicoId, queryParams);
        return Ok(data);
    }

    // GET api/v1/fornecedores/{id}
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var sindicoId = await _currentUser.GetSindicoScopeIdAsync();
        var data = await _service.GetByIdAsync(id, sindicoId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    // POST api/v1/fornecedores
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFornecedorRequest request)
    {
        var sindicoId = await _currentUser.GetSindicoScopeIdAsync();
        var data = await _service.CreateAsync(request, sindicoId);
        return StatusCode(201, ApiResponse<object>.Ok(data, "Fornecedor criado com sucesso"));
    }

    // PUT api/v1/fornecedores/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateFornecedorRequest request)
    {
        var sindicoId = await _currentUser.GetSindicoScopeIdAsync();
        var data = await _service.UpdateAsync(id, request, sindicoId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    // DELETE api/v1/fornecedores/{id}
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var sindicoId = await _currentUser.GetSindicoScopeIdAsync();
        await _service.DeleteAsync(id, sindicoId);
        return Ok(ApiResponse<object?>.Ok(null, "Fornecedor removido com sucesso"));
    }
}

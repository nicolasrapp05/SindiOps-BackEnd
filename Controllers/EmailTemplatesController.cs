using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SindiCore.API.DTOs.Requests;
using SindiCore.API.Helpers;
using SindiCore.API.Services.Interfaces;

namespace SindiCore.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/email-templates")]
public class EmailTemplatesController : ControllerBase
{
    private readonly IEmailTemplateService _service;

    public EmailTemplatesController(IEmailTemplateService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EmailTemplateQueryParams queryParams)
    {
        var userId = User.GetUserId();
        var data = await _service.GetAllAsync(userId, queryParams);
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var userId = User.GetUserId();
        var data = await _service.GetByIdAsync(id, userId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEmailTemplateRequest request)
    {
        var userId = User.GetUserId();
        var data = await _service.CreateAsync(request, userId);
        return StatusCode(201, ApiResponse<object>.Ok(data, "Template criado com sucesso"));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateEmailTemplateRequest request)
    {
        var userId = User.GetUserId();
        var data = await _service.UpdateAsync(id, request, userId);
        return Ok(ApiResponse<object>.Ok(data, "Template atualizado com sucesso"));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = User.GetUserId();
        try
        {
            await _service.DeleteAsync(id, userId);
            return Ok(ApiResponse<object?>.Ok(null, "Template removido com sucesso"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<object>.Fail(ex.Message));
        }
    }
}

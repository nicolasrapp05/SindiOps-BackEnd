using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.Helpers;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/email-logs")]
public class EmailLogsController : ControllerBase
{
    private readonly IEmailLogService _service;

    public EmailLogsController(IEmailLogService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] EmailLogQueryParams queryParams)
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
}

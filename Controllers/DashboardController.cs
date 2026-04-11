using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SindiCore.API.Helpers;
using SindiCore.API.Services.Interfaces;

namespace SindiCore.API.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;

    public DashboardController(IDashboardService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid? condominioId)
    {
        var userId = User.GetUserId();
        var data = await _service.GetDashboardAsync(userId, condominioId);
        return Ok(ApiResponse<object>.Ok(data));
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SindiOps.API.Authorization;
using SindiOps.API.Helpers;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Controllers;

[Authorize]
[RequireAllCargo]
[ApiController]
[Route("api/v1/dashboard")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _service;
    private readonly ICurrentUserService _currentUser;

    public DashboardController(IDashboardService service, ICurrentUserService currentUser)
    {
        _service = service;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid? condominioId)
    {
        var userId = User.GetUserId();
        var cargo = await _currentUser.GetCargoAsync();
        var data = await _service.GetDashboardAsync(userId, cargo, condominioId);
        return Ok(ApiResponse<object>.Ok(data));
    }
}

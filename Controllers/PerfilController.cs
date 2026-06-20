using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SindiOps.API.Authorization;
using SindiOps.API.DTOs.Requests;
using SindiOps.API.Helpers;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Controllers;

[Authorize]
[RequireAllCargo]
[ApiController]
[Route("api/v1/perfil")]
public class PerfilController : ControllerBase
{
    private readonly IPerfilService _service;

    public PerfilController(IPerfilService service)
    {
        _service = service;
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetMe()
    {
        var userId = User.GetUserId();
        var data = await _service.GetMeAsync(userId);
        return Ok(ApiResponse<object>.Ok(data));
    }

    [HttpPatch("me")]
    public async Task<IActionResult> UpdateMe([FromBody] UpdatePerfilRequest request)
    {
        var userId = User.GetUserId();
        var data = await _service.UpdateMeAsync(userId, request);
        return Ok(ApiResponse<object>.Ok(data, "Perfil atualizado com sucesso"));
    }
}

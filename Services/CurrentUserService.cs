using SindiOps.API.Constants;
using SindiOps.API.Helpers;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId =>
        _httpContextAccessor.HttpContext?.User.GetUserId()
        ?? throw new UnauthorizedAccessException("Usuário não autenticado");

    public string Cargo =>
        _httpContextAccessor.HttpContext?.User.GetCargo() ?? string.Empty;

    public bool IsSindico => Cargo == CargoConstants.Sindico;
}

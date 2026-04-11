using SindiCore.API.Constants;
using SindiCore.API.Helpers;
using SindiCore.API.Services.Interfaces;

namespace SindiCore.API.Services;

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

using SindiOps.API.Constants;
using SindiOps.API.Helpers;
using SindiOps.API.Infrastructure.Data;
using SindiOps.API.Services.Interfaces;

namespace SindiOps.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly SindiOpsDbContext _db;
    private string? _cargo;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor, SindiOpsDbContext db)
    {
        _httpContextAccessor = httpContextAccessor;
        _db = db;
    }

    public Guid UserId =>
        _httpContextAccessor.HttpContext?.User.GetUserId()
        ?? throw new UnauthorizedAccessException("Usuário não autenticado");

    public async Task<string> GetCargoAsync(CancellationToken ct = default)
    {
        if (_cargo is not null)
            return _cargo;

        _cargo = await UserCargoResolver.ResolveAsync(
            _db,
            UserId,
            _httpContextAccessor.HttpContext?.User,
            ct);

        return _cargo;
    }

    public string Cargo =>
        _cargo ?? _httpContextAccessor.HttpContext?.User.GetCargo() ?? string.Empty;

    public async Task<bool> IsSindicoAsync(CancellationToken ct = default) =>
        await GetCargoAsync(ct) == CargoConstants.Sindico;
}

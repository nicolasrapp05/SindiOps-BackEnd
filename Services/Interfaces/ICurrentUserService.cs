namespace SindiOps.API.Services.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string Cargo { get; }
    Task<string> GetCargoAsync(CancellationToken ct = default);
    Task<bool> IsSindicoAsync(CancellationToken ct = default);
}

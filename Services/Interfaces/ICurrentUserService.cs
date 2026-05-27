namespace SindiOps.API.Services.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string Cargo { get; }
    bool IsSindico { get; }
}

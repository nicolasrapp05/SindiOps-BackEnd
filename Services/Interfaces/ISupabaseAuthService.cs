namespace SindiOps.API.Services.Interfaces;

using SindiOps.API.Infrastructure.Auth;

public interface ISupabaseAuthService
{
    Task CreateUserAsync(Guid userId, string email, string nome, string cargo);
    Task CreateUserWithPasswordAsync(
        Guid userId, string email, string password, string nome, string cargo);
    Task SyncUserMetadataAsync(Guid userId, string nome, string cargo);
    Task<RecoveryLinkData?> GenerateRecoveryLinkAsync(string email, string redirectTo);
    Task DeleteUserAsync(Guid userId);
    Task<DateTimeOffset?> GetLastSignInAtAsync(Guid userId);
}

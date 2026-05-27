namespace SindiOps.API.Infrastructure.Email;

public interface IEmailService
{
    Task<bool> SendAsync(string to, string subject, string htmlBody);
}

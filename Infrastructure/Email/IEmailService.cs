namespace SindiOps.API.Infrastructure.Email;

public interface IEmailService
{
    Task<bool> SendAsync(string to, string subject, string plainBody);
    Task<bool> SendAuthHtmlAsync(string to, string subject, string htmlContent, string plainBody);
}

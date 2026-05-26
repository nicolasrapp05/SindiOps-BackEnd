using Resend;

namespace SindiCore.API.Infrastructure.Email;

public class ResendEmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly string _fromAddress;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(
        IResend resend,
        IConfiguration configuration,
        ILogger<ResendEmailService> logger)
    {
        _resend = resend;
        _fromAddress = configuration["Resend:FromAddress"]!;
        _logger = logger;
    }

    public async Task<bool> SendAsync(string to, string subject, string htmlBody)
    {
        try
        {
            var message = new EmailMessage
            {
                From = _fromAddress,
                Subject = subject,
                HtmlBody = htmlBody
            };

            message.To.Add(to);

            await _resend.EmailSendAsync(message);

            _logger.LogInformation("Email enviado para {To} | assunto: {Subject}", to, subject);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar email para {To} | assunto: {Subject}", to, subject);
            return false;
        }
    }
}

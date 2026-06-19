using Resend;

namespace SindiOps.API.Infrastructure.Email;

public class ResendEmailService : IEmailService
{
    private readonly IResend _resend;
    private readonly string _from;
    private readonly string? _replyTo;
    private readonly ILogger<ResendEmailService> _logger;

    public ResendEmailService(
        IResend resend,
        IConfiguration configuration,
        ILogger<ResendEmailService> logger)
    {
        _resend = resend;
        _logger = logger;

        var fromAddress = configuration["Resend:FromAddress"]
            ?? throw new InvalidOperationException("Resend:FromAddress não configurado.");
        var fromName = configuration["Resend:FromName"] ?? "SindiOps";
        _from = FormatFrom(fromName, fromAddress);
        _replyTo = configuration["Resend:ReplyTo"] ?? fromAddress;
    }

    public async Task<bool> SendAsync(string to, string subject, string plainBody)
    {
        try
        {
            var (html, text) = EmailBodyBuilder.Build(plainBody);

            return await SendMessageAsync(to, subject, html, text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar email para {To} | assunto: {Subject}", to, subject);
            return false;
        }
    }

    public async Task<bool> SendAuthHtmlAsync(string to, string subject, string htmlContent, string plainBody)
    {
        try
        {
            var (html, text) = EmailBodyBuilder.WrapAuthHtml(htmlContent, plainBody);

            return await SendMessageAsync(to, subject, html, text);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao enviar email para {To} | assunto: {Subject}", to, subject);
            return false;
        }
    }

    private async Task<bool> SendMessageAsync(string to, string subject, string html, string text)
    {
        var message = new EmailMessage
        {
            From = _from,
            ReplyTo = _replyTo,
            Subject = subject,
            HtmlBody = html,
            TextBody = text,
        };

        message.To.Add(to);

        await _resend.EmailSendAsync(message);

        _logger.LogInformation("Email enviado para {To} | assunto: {Subject} | de: {From}", to, subject, _from);
        return true;
    }

    private static string FormatFrom(string name, string address)
    {
        if (address.Contains('<', StringComparison.Ordinal))
            return address;

        var safeName = name.Replace("\"", "", StringComparison.Ordinal).Trim();
        return $"{safeName} <{address.Trim()}>";
    }
}

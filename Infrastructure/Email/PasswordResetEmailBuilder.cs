using System.Net;

namespace SindiOps.API.Infrastructure.Email;

public static class PasswordResetEmailBuilder
{
    public static (string HtmlContent, string PlainBody) Build(string email, string otp, string frontendUrl)
    {
        var encodedEmail = WebUtility.HtmlEncode(email);
        var encodedOtp = WebUtility.HtmlEncode(otp);
        var resetUrl = $"{frontendUrl.TrimEnd('/')}/redefinir-senha";

        var htmlContent = $"""
            <p style="margin:0 0 16px;font-size:20px;font-weight:700;color:#18181b;">Redefinir sua senha</p>
            <p style="margin:0 0 16px;">Olá,</p>
            <p style="margin:0 0 20px;">
              Recebemos uma solicitação para redefinir a senha da conta
              <strong style="color:#18181b;">{encodedEmail}</strong>.
            </p>
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="margin:0 0 24px;background-color:#f0fdf4;border:1px solid #bbf7d0;border-radius:8px;">
              <tr>
                <td style="padding:16px 20px;text-align:center;">
                  <p style="margin:0 0 8px;font-size:13px;color:#166534;font-weight:600;text-transform:uppercase;letter-spacing:0.05em;">
                    Código de verificação
                  </p>
                  <p style="margin:0;font-size:32px;font-weight:700;letter-spacing:0.2em;color:#047857;font-family:Consolas,Monaco,monospace;">
                    {encodedOtp}
                  </p>
                </td>
              </tr>
            </table>
            <p style="margin:0 0 12px;">Acesse a página abaixo e informe este código junto com sua nova senha:</p>
            <p style="margin:0 0 20px;">
              <a href="{resetUrl}" style="display:inline-block;padding:12px 24px;background-color:#047857;color:#ffffff;text-decoration:none;border-radius:8px;font-weight:600;">
                Abrir página de redefinição
              </a>
            </p>
            <p style="margin:0 0 16px;font-size:13px;color:#71717a;">
              <strong>Importante:</strong> use apenas o código acima. Não utilize links de emails anteriores.
              O código expira em algumas horas e só pode ser usado uma vez.
            </p>
            <p style="margin:0;">Se você não solicitou esta alteração, ignore esta mensagem — sua senha permanece a mesma.</p>
            """;

        var plainBody = $"""
            Redefinir sua senha — SindiOps

            Olá,

            Recebemos uma solicitação para redefinir a senha da conta {email}.

            Código de verificação: {otp}

            Acesse {resetUrl} e informe este código junto com sua nova senha.

            Importante: use apenas o código acima. Não utilize links de emails anteriores.
            O código expira em algumas horas e só pode ser usado uma vez.

            Se você não solicitou esta alteração, ignore esta mensagem.
            """;

        return (htmlContent, plainBody);
    }
}

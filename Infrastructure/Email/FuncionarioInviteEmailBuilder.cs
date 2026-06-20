using System.Net;

namespace SindiOps.API.Infrastructure.Email;

public static class FuncionarioInviteEmailBuilder
{
    public static (string HtmlContent, string PlainBody) Build(
        string nome, string email, string otp, string frontendUrl, string hashedToken)
    {
        var encodedNome = WebUtility.HtmlEncode(nome);
        var encodedEmail = WebUtility.HtmlEncode(email);
        var encodedOtp = WebUtility.HtmlEncode(otp);
        var setupUrl = RecoveryUrlBuilder.Build(frontendUrl, "primeiro-acesso", hashedToken);

        var htmlContent = $"""
            <p style="margin:0 0 16px;font-size:20px;font-weight:700;color:#18181b;">Bem-vindo ao SindiOps</p>
            <p style="margin:0 0 16px;">Olá, <strong style="color:#18181b;">{encodedNome}</strong>.</p>
            <p style="margin:0 0 20px;">
              Você foi convidado para acessar a plataforma SindiOps com a conta
              <strong style="color:#18181b;">{encodedEmail}</strong>.
            </p>
            <p style="margin:0 0 20px;">
              <a href="{setupUrl}" style="display:inline-block;padding:12px 24px;background-color:#047857;color:#ffffff;text-decoration:none;border-radius:8px;font-weight:600;">
                Ativar meu acesso
              </a>
            </p>
            <p style="margin:0 0 12px;font-size:13px;color:#71717a;text-align:center;">ou use o código abaixo</p>
            <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="margin:0 0 24px;background-color:#f0fdf4;border:1px solid #bbf7d0;border-radius:8px;">
              <tr>
                <td style="padding:16px 20px;text-align:center;">
                  <p style="margin:0 0 8px;font-size:13px;color:#166534;font-weight:600;text-transform:uppercase;letter-spacing:0.05em;">
                    Código de ativação
                  </p>
                  <p style="margin:0;font-size:32px;font-weight:700;letter-spacing:0.2em;color:#047857;font-family:Consolas,Monaco,monospace;">
                    {encodedOtp}
                  </p>
                </td>
              </tr>
            </table>
            <p style="margin:0 0 16px;font-size:13px;color:#71717a;">
              <strong>Importante:</strong> use apenas este email. O código expira em algumas horas e só pode ser usado uma vez.
            </p>
            <p style="margin:0;">Se você não reconhece este convite, ignore esta mensagem.</p>
            """;

        var plainBody = $"""
            Bem-vindo ao SindiOps

            Olá, {nome}.

            Você foi convidado para acessar a plataforma SindiOps com a conta {email}.

            Clique no link para ativar seu acesso:
            {setupUrl}

            Ou use o código de ativação: {otp}

            Se você não reconhece este convite, ignore esta mensagem.
            """;

        return (htmlContent, plainBody);
    }
}

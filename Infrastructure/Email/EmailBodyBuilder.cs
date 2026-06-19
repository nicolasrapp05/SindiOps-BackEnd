using System.Net;

namespace SindiOps.API.Infrastructure.Email;

public static class EmailBodyBuilder
{
    private const string DefaultFooter =
        "Comunicação oficial enviada pelo condomínio via SindiOps.<br />Por favor, não responda diretamente a este e-mail automático.";

    private const string AuthFooter =
        "Email automático enviado pelo SindiOps.<br />Por favor, não responda diretamente a este endereço.";

    public static (string HtmlBody, string TextBody) Build(string plainBody) =>
        WrapHtml(
            WebUtility.HtmlEncode(plainBody.Trim()).Replace("\n", "<br />", StringComparison.Ordinal),
            plainBody.Trim(),
            DefaultFooter);

    public static (string HtmlBody, string TextBody) WrapAuthHtml(string innerHtml, string plainBody) =>
        WrapHtml(innerHtml, plainBody, AuthFooter);

    private static (string HtmlBody, string TextBody) WrapHtml(
        string innerHtml,
        string plainBody,
        string footer)
    {
        var html = $"""
            <!DOCTYPE html>
            <html lang="pt-BR">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1" />
              <meta name="color-scheme" content="light" />
              <meta name="supported-color-schemes" content="light" />
              <title>SindiOps</title>
            </head>
            <body style="margin:0;padding:0;background-color:#f4f4f5;font-family:Arial,Helvetica,sans-serif;color:#18181b;">
              <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="background-color:#f4f4f5;padding:24px 16px;">
                <tr>
                  <td align="center">
                    <table role="presentation" width="100%" cellspacing="0" cellpadding="0" style="max-width:560px;background-color:#ffffff;border-radius:12px;border:1px solid #e4e4e7;overflow:hidden;">
                      <tr>
                        <td style="padding:20px 24px;background-color:#047857;color:#ffffff;font-size:18px;font-weight:700;letter-spacing:0.02em;">
                          SindiOps
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:24px;font-size:15px;line-height:1.6;color:#3f3f46;">
                          {innerHtml}
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:16px 24px 24px;font-size:12px;line-height:1.5;color:#71717a;border-top:1px solid #f4f4f5;">
                          {footer}
                        </td>
                      </tr>
                    </table>
                  </td>
                </tr>
              </table>
            </body>
            </html>
            """;

        return (html, plainBody.Trim());
    }
}

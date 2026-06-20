namespace SindiOps.API.Helpers;

public static class FrontendUrlResolver
{
    public static string Resolve(IConfiguration configuration) =>
        configuration["Frontend:BaseUrl"]
        ?? configuration["Cors:AllowedOrigin"]
        ?? "http://localhost:5173";
}

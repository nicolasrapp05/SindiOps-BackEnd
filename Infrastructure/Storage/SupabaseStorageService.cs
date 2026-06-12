using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentValidation;
using FluentValidation.Results;

namespace SindiOps.API.Infrastructure.Storage;

public class SupabaseStorageService : IStorageService
{
    private readonly HttpClient _httpClient;
    private readonly string _baseUrl;
    private readonly string _supabaseUrl;
    private readonly string _bucket;

    private static readonly HashSet<string> AllowedMimeTypes =
    [
        "image/jpeg",
        "image/png",
        "image/webp",
        "video/mp4",
        "video/quicktime"
    ];

    private const long MaxFileSizeBytes = 50L * 1024 * 1024; // 50 MB

    public SupabaseStorageService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _bucket = configuration["Supabase:StorageBucket"]!;

        var supabaseUrl = configuration["Supabase:Url"]!.TrimEnd('/');
        var serviceRoleKey = configuration["Supabase:ServiceRoleKey"]!;

        _supabaseUrl = supabaseUrl;
        _baseUrl = $"{supabaseUrl}/storage/v1";
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", serviceRoleKey);
        _httpClient.DefaultRequestHeaders.Add("apikey", serviceRoleKey);
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, string contentType)
    {
        ValidateMimeType(contentType);
        ValidateFileSize(stream);

        var url = $"{_baseUrl}/object/{_bucket}/{fileName}";

        using var content = new StreamContent(stream);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        var response = await _httpClient.PutAsync(url, content);
        response.EnsureSuccessStatusCode();

        return fileName;
    }

    public async Task<string> GetSignedUrlAsync(string filePath, int expiresInSeconds = 3600)
    {
        var url = $"{_baseUrl}/object/sign/{_bucket}/{filePath}";
        var body = JsonSerializer.Serialize(new { expiresIn = expiresInSeconds });

        using var content = new StringContent(body, Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        var signedPath = doc.RootElement.GetProperty("signedURL").GetString()!;

        // O Supabase pode retornar três formatos distintos:
        //   1. URL absoluta: "https://xxx.supabase.co/storage/v1/object/sign/..."  → usar como está
        //   2. Relativo ao domínio: "/storage/v1/object/sign/..."                  → prefixar com supabaseUrl
        //   3. Relativo ao base path: "/object/sign/..."                           → prefixar com _baseUrl (inclui /storage/v1)
        if (signedPath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            return signedPath;

        if (signedPath.StartsWith("/storage/", StringComparison.OrdinalIgnoreCase))
            return $"{_supabaseUrl}{signedPath}";

        // caso /object/sign/... — relativo a /storage/v1
        return $"{_baseUrl}{signedPath}";
    }

    public async Task DeleteAsync(string filePath)
    {
        var url = $"{_baseUrl}/object/{_bucket}/{filePath}";
        var response = await _httpClient.DeleteAsync(url);
        response.EnsureSuccessStatusCode();
    }

    // ── helpers de validação ────────────────────────────────────────────────

    private static void ValidateMimeType(string contentType)
    {
        if (!AllowedMimeTypes.Contains(contentType))
            throw new ValidationException(new[]
            {
                new ValidationFailure("arquivo",
                    "Tipo de arquivo não permitido. Aceitos: jpeg, png, webp, mp4, mov")
            });
    }

    private static void ValidateFileSize(Stream stream)
    {
        if (!stream.CanSeek)
            return;

        if (stream.Length > MaxFileSizeBytes)
            throw new ValidationException(new[]
            {
                new ValidationFailure("arquivo",
                    "O arquivo excede o tamanho máximo permitido de 50 MB")
            });
    }
}

public static class StorageFileNameHelper
{
    /// <summary>
    /// Gera o caminho do arquivo no bucket: {condominioId}/{ocorrenciaId}/{uuid}.{ext}
    /// </summary>
    public static string Build(Guid condominioId, Guid ocorrenciaId, string originalFileName)
    {
        var ext = Path.GetExtension(originalFileName).TrimStart('.');
        return $"{condominioId}/{ocorrenciaId}/{Guid.NewGuid()}.{ext}";
    }
}

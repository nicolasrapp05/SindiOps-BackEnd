namespace SindiOps.API.Infrastructure.Auth;

public class ForwardedHeadersSettings
{
    public const string SectionName = "ForwardedHeaders";

    public bool Enabled { get; set; }
}

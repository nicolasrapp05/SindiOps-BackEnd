namespace SindiOps.API.Helpers;

/// <summary>
/// Horário civil de Brasília. O Brasil não observa horário de verão.
/// </summary>
public static class BrazilTime
{
    private static readonly TimeZoneInfo Zone = ResolveZone();

    public static DateOnly Today => DateOnly.FromDateTime(Now);

    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, Zone);

    /// <summary>
    /// Relógio de parede sem fuso (o que o <c>datetime-local</c> envia) vira UTC.
    /// </summary>
    public static DateTime WallClockToUtc(DateTime unspecified)
    {
        var wall = DateTime.SpecifyKind(unspecified, DateTimeKind.Unspecified);
        return TimeZoneInfo.ConvertTimeToUtc(wall, Zone);
    }

    public static DateTime ToBrazil(DateTime utc)
    {
        var asUtc = utc.Kind == DateTimeKind.Utc
            ? utc
            : DateTime.SpecifyKind(utc, DateTimeKind.Utc);
        return TimeZoneInfo.ConvertTimeFromUtc(asUtc, Zone);
    }

    public static DateTime StartOfDayUtc(DateOnly day) =>
        WallClockToUtc(day.ToDateTime(TimeOnly.MinValue));

    public static DateTime EndOfDayUtc(DateOnly day) =>
        WallClockToUtc(day.ToDateTime(TimeOnly.MaxValue));

    private static TimeZoneInfo ResolveZone()
    {
        foreach (var id in new[] { "America/Sao_Paulo", "E. South America Standard Time" })
        {
            if (TimeZoneInfo.TryFindSystemTimeZoneById(id, out var zone))
                return zone;
        }

        return TimeZoneInfo.CreateCustomTimeZone(
            "America/Sao_Paulo",
            TimeSpan.FromHours(-3),
            "Brasília",
            "Brasília");
    }
}

namespace SindiCore.API.Infrastructure.Reports;

public interface IReportGenerator
{
    Task<byte[]> GenerateAsync(string tipo, object dados, string formato);
}

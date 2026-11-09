using SindiOps.API.Helpers;

namespace SindiOps.API.DTOs.Responses;

public class EmailLogContagemResponse
{
    public int TotalEntregues { get; set; }
    public int FalhasCriticas { get; set; }
    public int Total { get; set; }
}

public class EmailLogListaResponse : PaginatedResponse<EmailLogResponse>
{
    public EmailLogContagemResponse Resumo { get; set; } = new();
}

using System.Reflection;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Resend;
using SindiOps.API.Infrastructure.BackgroundJobs;
using SindiOps.API.Infrastructure.Data;
using SindiOps.API.Infrastructure.Email;
using SindiOps.API.Infrastructure.Reports;
using SindiOps.API.Infrastructure.Storage;
using SindiOps.API.Middleware;
using SindiOps.API.Services;
using SindiOps.API.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// ── DbContext ────────────────────────────────────────────────────────────────
builder.Services.AddDbContext<SindiOpsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ── AutoMapper ───────────────────────────────────────────────────────────────
builder.Services.AddAutoMapper(cfg => cfg.AddMaps(Assembly.GetExecutingAssembly()));

// ── FluentValidation ─────────────────────────────────────────────────────────
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ── JWT Authentication (valida token emitido pelo Supabase Auth) ─────────────
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = $"{builder.Configuration["Supabase:Url"]}/auth/v1";
        options.Audience = "authenticated";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Supabase:JwtSecret"]!)),
            ValidateIssuer = true,
            ValidIssuer = $"{builder.Configuration["Supabase:Url"]}/auth/v1",
            ValidateAudience = true,
            ValidAudience = "authenticated",
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// ── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", policy =>
        policy
            .WithOrigins(builder.Configuration["Cors:AllowedOrigin"]!)
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()));

// ── Swagger / OpenAPI ─────────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SíndiOps API", Version = "v1" });
});

// ── HttpContextAccessor ───────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();

// ── Resend (email transacional) ───────────────────────────────────────────────
builder.Services.AddOptions();
builder.Services.AddHttpClient<ResendClient>();
builder.Services.Configure<ResendClientOptions>(options =>
{
    options.ApiToken = builder.Configuration["Resend:ApiKey"]!;
});
builder.Services.AddTransient<IResend, ResendClient>();

// ── Serviços de infraestrutura ────────────────────────────────────────────────
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

// ── Serviços de domínio ───────────────────────────────────────────────────────
builder.Services.AddScoped<ICondominioService, CondominioService>();
builder.Services.AddScoped<IMoradorService, MoradorService>();
builder.Services.AddScoped<IFuncionarioService, FuncionarioService>();
builder.Services.AddScoped<IFornecedorService, FornecedorService>();
builder.Services.AddScoped<IContratoService, ContratoService>();
builder.Services.AddScoped<IManutencaoObrigatoriaService, ManutencaoObrigatoriaService>();
builder.Services.AddScoped<ISolicitacaoManutencaoService, SolicitacaoManutencaoService>();
builder.Services.AddScoped<ISolicitacaoCompraService, SolicitacaoCompraService>();
builder.Services.AddScoped<IOcorrenciaService, OcorrenciaService>();
builder.Services.AddScoped<IEmailTemplateService, EmailTemplateService>();
builder.Services.AddScoped<IEmailLogService, EmailLogService>();
builder.Services.AddScoped<IComunicacaoService, ComunicacaoService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();
builder.Services.AddScoped<IReportGenerator, ReportGenerator>();
builder.Services.AddScoped<IEmailService, ResendEmailService>();
builder.Services.AddScoped<ITemplateResolver, TemplateResolver>();
builder.Services.AddHttpClient<IStorageService, SupabaseStorageService>();
// IHttpClientFactory disponível para FuncionarioService (chamada à Admin API do Supabase)
builder.Services.AddHttpClient();

// ── Background Service — atualização diária de status de manutenções ─────────
builder.Services.AddHostedService<ManutencaoStatusJob>();

// ── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();
// ─────────────────────────────────────────────────────────────────────────────

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SíndiOps API v1"));
}

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();

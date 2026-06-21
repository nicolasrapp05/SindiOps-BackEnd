using System.Reflection;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Resend;
using SindiOps.API.Helpers;
using SindiOps.API.Infrastructure.Auth;
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

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = ValidationResponseFactory.Create;
});

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

// ── Swagger (Swashbuckle) ─────────────────────────────────────────────────────
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SíndiOps API", Version = "v1" });
});

// ── HttpContextAccessor ───────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();
builder.Services.Configure<PasswordResetRateLimitOptions>(
    builder.Configuration.GetSection(PasswordResetRateLimitOptions.SectionName));
builder.Services.Configure<ConviteResendRateLimitOptions>(
    builder.Configuration.GetSection(ConviteResendRateLimitOptions.SectionName));
builder.Services.Configure<CadastroSindicoRateLimitOptions>(
    builder.Configuration.GetSection(CadastroSindicoRateLimitOptions.SectionName));
builder.Services.Configure<ForwardedHeadersSettings>(
    builder.Configuration.GetSection(ForwardedHeadersSettings.SectionName));
builder.Services.AddSingleton<IPasswordResetRateLimiter, PasswordResetRateLimiter>();
builder.Services.AddSingleton<IConviteResendRateLimiter, ConviteResendRateLimiter>();
builder.Services.AddSingleton<ICadastroSindicoRateLimiter, CadastroSindicoRateLimiter>();

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
builder.Services.AddScoped<ISupabaseAuthService, SupabaseAuthService>();

// ── Serviços de domínio ───────────────────────────────────────────────────────
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPerfilService, PerfilService>();
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
builder.Services.AddHttpClient();

// ── Background jobs — atualização diária de status (manutenções e contratos) ─
builder.Services.AddHostedService<ManutencaoStatusJob>();
builder.Services.AddHostedService<ContratoStatusJob>();

// ── Controllers ──────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// ─────────────────────────────────────────────────────────────────────────────
var app = builder.Build();
// ─────────────────────────────────────────────────────────────────────────────

var forwardedHeadersEnabled = app.Configuration
    .GetSection(ForwardedHeadersSettings.SectionName)
    .GetValue<bool>("Enabled");

if (forwardedHeadersEnabled)
{
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    });
}

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

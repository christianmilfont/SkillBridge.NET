using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Instrumentation.Process;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SkillBridge_dotnet.Api.Data;
using SkillBridge_dotnet.Api.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ===========================================================
// 🔹 Identificação do serviço para o OpenTelemetry
// ===========================================================
var serviceName = "SkillBridge.API";
var serviceVersion = "1.0.0";

// 🔹 Configura o Resource compartilhado (nome, versão e ambiente)
var resourceBuilder = ResourceBuilder.CreateDefault()
    .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
    .AddAttributes(new Dictionary<string, object>
    {
        ["deployment.environment"] = builder.Environment.EnvironmentName,
        ["host.name"] = Environment.MachineName
    });

// ===========================================================
// 🔹 Controllers e Swagger
// ===========================================================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddScoped<IMatchingService, MatchingService>();
builder.Services.AddScoped<RecommendationService>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApiVersioning(o =>
{
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    o.ReportApiVersions = true;
});

// ===========================================================
// 🔹 Banco de Dados MySQL
// ===========================================================
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 36))
    )
);

// ===========================================================
// 🔹 Health Checks
// ===========================================================
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("Database");

// ===========================================================
// 🔹 Configuração de Autenticação JWT
// ===========================================================
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey))
{
    throw new Exception("❌ JWT Key não configurada no appsettings.json");
}

var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ClockSkew = TimeSpan.Zero // evita tolerância no tempo de expiração
        };
    });

// ===========================================================
// 🔹 Autorização
// ===========================================================
builder.Services.AddAuthorization();

// ===========================================================
// 🔹 OpenTelemetry (Tracing + Metrics)
// ===========================================================
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName))
    .WithTracing(t =>
    {
        t
            .SetResourceBuilder(resourceBuilder)
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/healthz");
            })
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation(opt => { opt.SetDbStatementForText = true; })
            .AddOtlpExporter(); // envia para o OTLP (New Relic/Azure)
    })
    .WithMetrics(m =>
    {
        m
            .SetResourceBuilder(resourceBuilder)
            .AddRuntimeInstrumentation()
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddProcessInstrumentation()
            .AddOtlpExporter();
    });

// ===========================================================
// 🔹 Build do app
// ===========================================================
var app = builder.Build();

// ===========================================================
// 🔹 Swagger apenas em desenvolvimento
// ===========================================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ===========================================================
// 🔹 Middleware de segurança
// ===========================================================
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// ===========================================================
// 🔹 Endpoints
// ===========================================================
app.MapHealthChecks("/healthz");
app.MapControllers();

app.Run();

// Necessário para testes de integração
public partial class Program { }

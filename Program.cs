using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Instrumentation.Process;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SkillBridge_dotnet.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// 🔹 Identificação do serviço para o OpenTelemetry
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

// 🔹 Controllers e Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApiVersioning(o =>
{
    o.AssumeDefaultVersionWhenUnspecified = true;
    o.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
    o.ReportApiVersions = true;
});

// 🔹 Banco de Dados MySQL
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 36))
    )
);

// 🔹 Health Checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("Database");

// 🔹 Configuração completa do OpenTelemetry
builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService(serviceName))
    .WithTracing(t =>
    {
        t
            .SetResourceBuilder(resourceBuilder)
            .AddAspNetCoreInstrumentation(options =>
            {
                options.RecordException = true;
                options.Filter = ctx => !ctx.Request.Path.StartsWithSegments("/healthz"); // ignora healthcheck
            })
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation(opt =>
            {
                opt.SetDbStatementForText = true;
            })
            .AddOtlpExporter(); // envia para o OTLP (padrão NewRelic/Azure)
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

var app = builder.Build();

// 🔹 Swagger apenas no ambiente de desenvolvimento
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// 🔹 Endpoints
app.MapHealthChecks("/healthz");
app.MapControllers();

app.Run();

// Necessário para testes de integração
public partial class Program { }

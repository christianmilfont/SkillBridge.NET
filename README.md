# SkillBridge .NET API

## Visão Geral

**SkillBridge** é uma API desenvolvida em **.NET 8 (ASP.NET Core)** que faz parte da plataforma **SkillBridge** — um ecossistema de requalificação profissional e conexão entre aprendizado e mercado de trabalho.  
![Imagem do WhatsApp de 2025-11-23 à(s) 17 49 10_f2999625](https://github.com/user-attachments/assets/203dc18e-15e2-4a7e-9810-cb986c9a1db7)

O projeto tem como objetivo **conectar perfis de usuários a cursos, competências e vagas**, permitindo uma **trilha de aprendizado personalizada** gerada por **inteligência artificial (IA)**.

Esta API é o **núcleo principal (backend)** do sistema, responsável pelos **CRUDs** (Create, Read, Update, Delete) das entidades principais e pela **instrumentação de observabilidade completa com OpenTelemetry**, garantindo rastreabilidade e métricas em produção.


Nossa proposta e o Product Deck:
<img width="926" height="574" alt="image" src="https://github.com/user-attachments/assets/ef9801d1-3d4d-4741-9dff-88f6b45f721e" />

---

## Arquitetura
<img width="990" height="666" alt="image" src="https://github.com/user-attachments/assets/65a8aef5-4241-4b10-b69d-dc1469ce95ed" />

A aplicação segue uma arquitetura **modular e escalável**, com integração nativa ao **MySQL** e suporte total a **telemetria (traces, métricas, logs)** via **OpenTelemetry**.

### Camadas Principais

- **API (Controllers):** ponto de entrada HTTP.
- **Data Layer:** contexto do banco de dados com EF Core.
- **Entities/Models:** classes representando os domínios do sistema.
- **Observability:** configuração completa de tracing e métricas.
- **Health Checks:** endpoint para monitoramento de saúde da aplicação.

---

## Entidades Principais
```bash
  A base de dados contém as seguintes tabelas/entidades:
  
  | Entidade     | Descrição                                                                 |
  |---------------|---------------------------------------------------------------------------|
  | **User**      | Representa o usuário do sistema (login, senha, role).                     |
  | **Profile**   | Perfil associado ao usuário, com dados pessoais e profissionais.          |
  | **Competency**| Competências/habilidades cadastradas e associadas a perfis e cursos.     |
  | **Course**    | Cursos curtos de requalificação vinculados a competências.                |
  | **Vacancy**   | Vagas ou mentorias publicadas por empresas ou parceiros.                  |
  | **Enrollment**| Matrículas de usuários em cursos (controle de progresso e status).        |
```

---

## Tecnologias Utilizadas

```bash
  | Categoria | Tecnologia |
  |------------|-------------|
  | **Linguagem** | C# (.NET 8) |
  | **Framework Web** | ASP.NET Core |
  | **Banco de Dados** | MySQL (via `Pomelo.EntityFrameworkCore.MySql`) |
  | **ORM** | Entity Framework Core |
  | **Documentação** | Swagger / OpenAPI |
  | **Observabilidade** | OpenTelemetry (Traces, Metrics, Logs) |
  | **Infraestrutura** | Azure PaaS / Docker / CI/CD |
  | **Health Monitoring** | ASP.NET HealthChecks |
```

---

## Program.cs — Explicação Completa

O arquivo `Program.cs` contém toda a configuração essencial da API.

### 1. Configuração Base
```csharp
  var builder = WebApplication.CreateBuilder(args);
```


Cria e inicializa a aplicação ASP.NET Core.


---

### 2. Identidade do Serviço para OpenTelemetry
Define nome, versão e metadados que serão usados nos traces e métricas.

```csharp
  var serviceName = "SkillBridge.API";
  var serviceVersion = "1.0.0";
```

---

### 3. ResourceBuilder (metadados da aplicação)
Define o nome do serviço, ambiente (dev/prod) e o nome do host.

```csharp
  var resourceBuilder = ResourceBuilder.CreateDefault()
      .AddService(serviceName: serviceName, serviceVersion: serviceVersion)
      .AddAttributes(new Dictionary<string, object>
      {
          ["deployment.environment"] = builder.Environment.EnvironmentName,
          ["host.name"] = Environment.MachineName
      });
```

---


### 4. Serviços Essenciais
Adiciona Controllers, Swagger e versionamento de API.

```csharp
  builder.Services.AddControllers();
  builder.Services.AddEndpointsApiExplorer();
  builder.Services.AddSwaggerGen();
  builder.Services.AddApiVersioning(o =>
  {
      o.AssumeDefaultVersionWhenUnspecified = true;
      o.DefaultApiVersion = new Microsoft.AspNetCore.Mvc.ApiVersion(1, 0);
      o.ReportApiVersions = true;
  });
```

---

### 5. Banco de Dados MySQL
Configura o Entity Framework Core para uso com MySQL 8.0.36.

```csharp
  builder.Services.AddDbContext<AppDbContext>(opt =>
      opt.UseMySql(
          builder.Configuration.GetConnectionString("DefaultConnection"),
          new MySqlServerVersion(new Version(8, 0, 36))
      )
  );
```

---

### 6. Health Checks
Permite o monitoramento de saúde da aplicação e do banco de dados.

```csharp
  builder.Services.AddHealthChecks()
      .AddDbContextCheck<AppDbContext>("Database");
```

---

### 7. OpenTelemetry (Tracing + Metrics)
Configuração completa para telemetria distribuída.

  ```csharp
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
              .AddEntityFrameworkCoreInstrumentation(opt => opt.SetDbStatementForText = true)
              .AddOtlpExporter();
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
```

Benefício: envia métricas e traces automaticamente para New Relic, Azure Monitor, ou qualquer endpoint OTLP.

---

### 8. Middleware e Endpoints
Define Swagger, Health Check e Controllers.

```csharp
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/healthz");
app.MapControllers();
```

---


## Como Executar o Projeto
Restaurar dependências
```bash
  dotnet restore
```
### Criar o banco de dados (MySQL)
Certifique-se de que o MySQL está rodando e o connection string está correto em:
appsettings.json

```json
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=skillbridge_db;User=root;Password=senha;"
  }
```
### Criar migrações
```bash
  dotnet ef migrations add InitialCreate
  dotnet ef database update
```
### Executar a API
```bash
  dotnet run
```

### Acessar no navegador
```
  API Swagger: http://localhost:5000/swagger
  
  Health Check: http://localhost:5000/healthz
```


#### Autor
Christian Milfont — Desenvolvedor Full Stack / FIAP
Projeto desenvolvido para a Global Solution SkillBridge (2025).

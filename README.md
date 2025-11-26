# SkillBridge .NET API
Grupo:
Christian Milfont Rm555345
Iago Victor Rm558450

## Visão Geral

**SkillBridge** é uma API desenvolvida em **.NET 8 (ASP.NET Core)** que faz parte da plataforma **SkillBridge** — um ecossistema de requalificação profissional e conexão entre aprendizado e mercado de trabalho.  
![Imagem do WhatsApp de 2025-11-23 à(s) 17 49 10_f2999625](https://github.com/user-attachments/assets/203dc18e-15e2-4a7e-9810-cb986c9a1db7)

O projeto tem como objetivo **conectar perfis de usuários a cursos, competências e vagas**, permitindo uma **trilha de aprendizado personalizada** gerada por **inteligência artificial (IA)**.

Esta API é o **núcleo principal (backend)** do sistema, responsável pelos **CRUDs** (Create, Read, Update, Delete) das entidades principais e pela **instrumentação de observabilidade completa com OpenTelemetry**, garantindo rastreabilidade e métricas em produção.


Nossa proposta e o Product Deck:
<img width="926" height="574" alt="image" src="https://github.com/user-attachments/assets/ef9801d1-3d4d-4741-9dff-88f6b45f721e" />

---

## Arquitetura
<img width="997" height="655" alt="image" src="https://github.com/user-attachments/assets/bc17434c-1e7e-4620-95d5-c7226eb561ef" />

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

## CRUD COMPLETO PARA O README.md

Abaixo está a lista de todas as rotas CRUD referentes às entidades presentes no seu banco:
```bash
Users

Profiles

Competencies

Courses

Vacancies

Enrollments

Recommendations

Relações N:N

course_competencies

profile_competencies

vacancy_competencies
```
**As rotas estão organizadas por entidade.**

1. USERS – CRUD
Create User

POST /api/users
```
{
  "email": "usuario1@exemplo.com",
  "username": "usuario1",
  "password": "123456",
  "role": "admin"
}
```
Get All Users

GET /api/users

Get User by ID

GET /api/users/{id}

Update User
```
PUT /api/users/{id}

{
  "email": "usuario.atualizado@exemplo.com",
  "username": "usuario1",
  "role": "admin"
}
```
Delete User
```
DELETE /api/users/{id}

Deleta automaticamente o Profile (CASCADE).
```
2. PROFILES – CRUD
 
```
Create Profile

POST /api/profiles

{
  "fullName": "Nome do Usuário 1",
  "bio": "Bio do usuário",
  "location": "São Paulo - SP",
  "userId": "ID_DO_USER"
}
```
Get All Profiles

GET /api/profiles

Get Profile by ID

GET /api/profiles/{id}
```
Update Profile

PUT /api/profiles/{id}

{
  "fullName": "Nome Atualizado",
  "bio": "Bio atualizada",
  "location": "Rio de Janeiro - RJ"
}
```
```
Delete Profile

DELETE /api/profiles/{id}
```
3. COMPETENCIES – CRUD
```
Create Competency

POST /api/competencies

{
  "name": "Lógica de Programação",
  "description": "Fundamentos de lógica",
  "recommendedLevel": "Beginner"
}
```
Get All

GET /api/competencies

Get by ID

GET /api/competencies/{id}
```
Update

PUT /api/competencies/{id}

{
  "description": "Descrição atualizada",
  "recommendedLevel": "Intermediate"
}
```
```
Delete

DELETE /api/competencies/{id}
```
4. COURSES – CRUD
```
Create Course

POST /api/courses

{
  "title": "Curso de APIs com .NET",
  "description": "Aprenda a criar APIs RESTful",
  "durationHours": 20,
  "price": 79.90
}
```
Get All Courses

GET /api/courses

Get by ID

GET /api/courses/{id}
```
Update

PUT /api/courses/{id}

{
  "price": 99.90,
  "durationHours": 24
}
```
```
Delete

DELETE /api/courses/{id}
```
5. VACANCIES – CRUD
```
Create Vacancy

POST /api/vacancies

{
  "title": "Desenvolvedor Back-end",
  "company": "TechCorp",
  "location": "São Paulo",
  "description": "Desenvolvimento de APIs",
  "salaryMin": 5000,
  "salaryMax": 9000,
  "status": "Open"
}
```
Get All

GET /api/vacancies

Get by ID

GET /api/vacancies/{id}
```
Update

PUT /api/vacancies/{id}

{
  "title": "Backend Pleno",
  "location": "Remoto",
  "status": "Closed"
}
```
```
Delete

DELETE /api/vacancies/{id}
```
6. ENROLLMENTS – CRUD

```
Create Enrollment

POST /api/enrollments

{
  "userId": "ID_USER",
  "courseId": "ID_COURSE",
  "status": "InProgress"
}
```
Get All

GET /api/enrollments

Get by ID

GET /api/enrollments/{id}
```
Update (Progress & Score)

PUT /api/enrollments/{id}

{
  "progress": 75.5,
  "score": 8.5,
  "status": "Completed"
}
```
```
Delete

DELETE /api/enrollments/{id}
```
7. RECOMMENDATIONS – CRUD

```
Create Recommendation

POST /api/recommendations

{
  "profileId": "ID_PROFILE",
  "courseId": "ID_COURSE",
  "vacancyId": "ID_VACANCY"
}
```
Get All

GET /api/recommendations

Get by ID

GET /api/recommendations/{id}
```
Delete

DELETE /api/recommendations/{id}
```
Geralmente não tem UPDATE pois são geradas automaticamente.

8. RELAÇÕES N:N (JOIN TABLES)
8.1 Course_Competencies

```
Create

POST /api/courses/{courseId}/competencies

{
  "competencyId": "ID_COMPETENCY",
  "coveragePercent": 80,
  "requiredLevel": 3
}
```
Get List

GET /api/courses/{courseId}/competencies
```
Delete

DELETE /api/courses/{courseId}/competencies/{competencyId}
```
8.2 Profile_Competencies

```
Create

POST /api/profiles/{profileId}/competencies

{
  "competencyId": "ID_COMPETENCY",
  "selfAssessedLevel": "Intermediate",
  "yearsExperience": 2
}
```
```
Get

GET /api/profiles/{profileId}/competencies
```
```
Update

PUT /api/profiles/{profileId}/competencies/{competencyId}

{
  "selfAssessedLevel": "Advanced",
  "yearsExperience": 4
}
```
```
Delete

DELETE /api/profiles/{profileId}/competencies/{competencyId}
```
8.3 Vacancy_Competencies

```
Create

POST /api/vacancies/{vacancyId}/competencies

{
  "competencyId": "ID_COMPETENCY",
  "isMandatory": true,
  "requiredLevel": "Senior"
}
```
List

GET /api/vacancies/{vacancyId}/competencies
```
Delete

DELETE /api/vacancies/{vacancyId}/competencies/{competencyId}
```



#### Autor
Christian Milfont — Desenvolvedor Full Stack / FIAP
Projeto desenvolvido para a Global Solution SkillBridge (2025).

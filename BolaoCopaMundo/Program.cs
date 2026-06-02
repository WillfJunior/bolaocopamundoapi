using System.Text;
using BolaoCopaMundo.Application.Services;
using BolaoCopaMundo.Infrastructure.Data;
using BolaoCopaMundo.Infrastructure.Data.Seeding;
using BolaoCopaMundo.Infrastructure.Jobs;
using BolaoCopaMundo.Infrastructure.Services;
using BolaoCopaMundo.Middlewares;
using FluentValidation;
using FluentValidation.AspNetCore;
using Hangfire;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ─── Entity Framework / PostgreSQL ───────────────────────────────────────────
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ─── Autenticação JWT ─────────────────────────────────────────────────────────
var jwtKey = builder.Configuration["Jwt:Secret"]
    ?? throw new InvalidOperationException("Jwt:Secret não configurado.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireClaim("admin", "true"));
});

// ─── Hangfire ─────────────────────────────────────────────────────────────────
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = 2;
    options.Queues = ["default"];
});

// ─── HTTP Clients ─────────────────────────────────────────────────────────────
builder.Services.AddHttpClient<FootballApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.football-data.org/v4/");
    client.DefaultRequestHeaders.Add("X-Auth-Token",
        builder.Configuration["FootballApi:ApiKey"] ?? string.Empty);
});

// ─── FluentValidation ─────────────────────────────────────────────────────────
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// ─── Serviços de Infraestrutura ───────────────────────────────────────────────
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<PushNotificationService>();

// ─── Serviços de Aplicação ────────────────────────────────────────────────────
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MatchService>();
builder.Services.AddScoped<PredictionService>();
builder.Services.AddScoped<RankingService>();
builder.Services.AddScoped<BolaoGroupService>();

// ─── Jobs Hangfire ────────────────────────────────────────────────────────────
builder.Services.AddScoped<UpdateMatchResultsJob>();
builder.Services.AddScoped<SendMatchRemindersJob>();
builder.Services.AddScoped<GenerateNextPhaseJob>();

// ─── Seeder ───────────────────────────────────────────────────────────────────
builder.Services.AddScoped<WorldCup2026Seeder>();

// ─── Controllers + Swagger ────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Bolão Copa do Mundo 2026",
        Version = "v1",
        Description = "API para gerenciamento de bolão da Copa do Mundo FIFA 2026"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

// ─── CORS (PWA) ───────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
{
    options.AddPolicy("PwaPolicy", policy =>
        policy.WithOrigins(
                builder.Configuration.GetSection("AllowedOrigins").Get<string[]>()
                ?? ["http://localhost:3000", "http://localhost:5173"])
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// ─── Migrations + Seed ────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    var seeder = scope.ServiceProvider.GetRequiredService<WorldCup2026Seeder>();
    await seeder.SeedAsync();
}

// ─── Pipeline ─────────────────────────────────────────────────────────────────
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Bolão Copa 2026 v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseStaticFiles();
app.UseCors("PwaPolicy");
app.UseAuthentication();
app.UseAuthorization();

// Hangfire Dashboard (apenas admin em produção)
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = [new HangfireAdminAuthorizationFilter()]
});

app.MapControllers();

// ─── Recurring Jobs ───────────────────────────────────────────────────────────
// Atualiza resultados via football-data.org a cada 5 minutos
RecurringJob.AddOrUpdate<UpdateMatchResultsJob>(
    "sync-match-results",
    job => job.ExecuteAsync(),
    "*/5 * * * *");

// Envia lembretes 1h antes dos jogos (verifica a cada 5 min)
RecurringJob.AddOrUpdate<SendMatchRemindersJob>(
    "send-match-reminders",
    job => job.ExecuteAsync(),
    "*/5 * * * *");

app.Run();

// ─── Filtro de autorização do Hangfire Dashboard ──────────────────────────────
public class HangfireAdminAuthorizationFilter : Hangfire.Dashboard.IDashboardAuthorizationFilter
{
    public bool Authorize(Hangfire.Dashboard.DashboardContext context)
    {
        // Acessa HttpContext via reflexão para compatibilidade entre versões do Hangfire
        var httpContext = context.GetType().GetProperty("HttpContext")?.GetValue(context) as HttpContext;

        if (httpContext is null) return false;
        if (!httpContext.User.Identity?.IsAuthenticated ?? true) return false;

        return httpContext.User.HasClaim("admin", "true");
    }
}

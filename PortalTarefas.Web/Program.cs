using System.Diagnostics;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PortalTarefas.Web.Configuracao;
using PortalTarefas.Web.Data;
using PortalTarefas.Web.Endpoints;
using PortalTarefas.Web.Filters;
using PortalTarefas.Web.Observabilidade;
using PortalTarefas.Web.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// =============== 1) SERVIÇOS ===============
builder.Services.AddProblemDetails();

builder.Services.AddControllersWithViews(opcoes =>
{
    opcoes.Filters.Add<LogAcaoFilter>();
    opcoes.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute());
});

builder.Services.AddDbContextPool<PortalTarefasDbContext>(opcoes =>
    opcoes.UseSqlite(builder.Configuration.GetConnectionString("PortalTarefasDb")));

builder.Services.AddScoped<ITarefaService, TarefaServiceEf>();

// ===== Roteiro 6: IDENTIDADE (mantida) =====
builder.Services.AddIdentityApiEndpoints<IdentityUser>(opcoes =>
{
    opcoes.Password.RequiredLength = 8;
    opcoes.Lockout.MaxFailedAccessAttempts = 5;
    opcoes.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<PortalTarefasDbContext>();

builder.Services.ConfigureApplicationCookie(opcoes =>
{
    opcoes.Cookie.HttpOnly = true;
    opcoes.Cookie.SameSite = SameSiteMode.Strict;
    opcoes.ExpireTimeSpan = TimeSpan.FromHours(1);
    opcoes.SlidingExpiration = true;
});

builder.Services.Configure<BearerTokenOptions>(IdentityConstants.BearerScheme, opcoes =>
    opcoes.BearerTokenExpiration = TimeSpan.FromMinutes(15));

// ===== Roteiro 6: AUTORIZAÇÃO (mantida) =====
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("PodeCriar", p => p.RequireRole("Usuario", "Administrador"))
    .AddPolicy("SomenteAdministrador", p => p.RequireRole("Administrador"))
    .AddPolicy("EditarOutraEquipe", p => p.RequireClaim("permissao", "editar-outras-equipes"));

builder.Services.AddValidation();
builder.Services.AddOpenApi();

builder.Services.ConfigureHttpJsonOptions(opcoes =>
{
    opcoes.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
    opcoes.SerializerOptions.NumberHandling = JsonNumberHandling.Strict;
});

builder.Services.AddCors(opcoes =>
    opcoes.AddPolicy("FrontEndLocal", politica => politica
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()));

// ===== Roteiro 7: LOGS =====
builder.Logging.AddSimpleConsole(opcoes =>
{
    opcoes.IncludeScopes = true;
    opcoes.SingleLine = true;
    opcoes.TimestampFormat = "HH:mm:ss ";
});

// ===== Roteiro 7: MÉTRICAS e TRACING com OpenTelemetry =====
builder.Services.AddOpenTelemetry()
    .ConfigureResource(recurso => recurso.AddService("PortalTarefas.Web"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddSource(Telemetria.Nome)
        .AddConsoleExporter())
    .WithMetrics(metricas => metricas
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddMeter(Telemetria.Nome)
        .AddConsoleExporter((_, leitor) =>
            leitor.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 15000));

// ===== Roteiro 7: HttpClient RESILIENTE para o "serviço externo" =====
// Roteiro 8: o endereço vem das opções JÁ VALIDADAS (ExternoOpcoes)
builder.Services.AddHttpClient("externo", (servicos, cliente) =>
        cliente.BaseAddress = new Uri(servicos.GetRequiredService<IOptions<ExternoOpcoes>>().Value.BaseUrl!))
    .AddStandardResilienceHandler(opcoes =>
    {
        opcoes.AttemptTimeout.Timeout = TimeSpan.FromSeconds(2);
        opcoes.Retry.MaxRetryAttempts = 2;
        opcoes.Retry.Delay = TimeSpan.FromMilliseconds(200);
        opcoes.Retry.DisableForUnsafeHttpMethods();
        opcoes.CircuitBreaker.MinimumThroughput = 5;
        opcoes.CircuitBreaker.FailureRatio = 0.5;
        opcoes.CircuitBreaker.BreakDuration = TimeSpan.FromSeconds(15);
    });

// ===== Roteiro 7: HEALTH CHECKS =====
builder.Services.AddHealthChecks()
    .AddDbContextCheck<PortalTarefasDbContext>("banco", tags: ["ready"])
    .AddCheck<ServicoExternoHealthCheck>("externo", tags: ["ready"]);

// ===== Roteiro 7: RATE LIMITING =====
builder.Services.AddRateLimiter(opcoes =>
{
    opcoes.AddFixedWindowLimiter("publico", janela =>
    {
        janela.PermitLimit = 5;
        janela.Window = TimeSpan.FromSeconds(10);
    });
    opcoes.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    opcoes.OnRejected = (contexto, _) =>
    {
        contexto.HttpContext.Response.Headers.RetryAfter = "10";
        contexto.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>()
            .LogWarning("Limite de requisições excedido em {Caminho}", contexto.HttpContext.Request.Path);
        return ValueTask.CompletedTask;
    };
});

// ===== Roteiro 7: OUTPUT CACHING =====
builder.Services.AddOutputCache();

// ===== Roteiro 8: VALIDAR A CONFIGURAÇÃO NA INICIALIZAÇÃO =====
// ValidateOnStart: se faltar algo obrigatório, a aplicação NEM SOBE, com a mensagem da classe de opções.
// Durante a geração do contrato OpenAPI no build (entrada "GetDocument.Insider") os segredos
// não existem, então a validação de partida só vale para a execução de verdade.
var gerandoContrato = Assembly.GetEntryAssembly()?.GetName().Name == "GetDocument.Insider";

var opcoesConexao = builder.Services.AddOptions<ConexoesOpcoes>()
    .BindConfiguration("ConnectionStrings").ValidateDataAnnotations();
var opcoesExterno = builder.Services.AddOptions<ExternoOpcoes>()
    .BindConfiguration("Externo").ValidateDataAnnotations();

if (!gerandoContrato)
{
    opcoesConexao.ValidateOnStart();
    opcoesExterno.ValidateOnStart();
}

var app = builder.Build();

// =============== 2) PIPELINE (em ordem) ===============

// [A] Nosso middleware de log — o PRIMEIRO da fila.
app.Use(async (context, next) =>
{
    var cronometro = Stopwatch.StartNew();
    var correlacao = Activity.Current?.TraceId.ToString() ?? context.TraceIdentifier;
    context.Response.OnStarting(() =>
    {
        context.Response.Headers["X-Correlation-Id"] = correlacao;
        return Task.CompletedTask;
    });

    await next(context);

    app.Logger.LogInformation("HTTP {Metodo} {Caminho} respondeu {Status} em {Ms} ms",
        context.Request.Method, context.Request.Path,
        context.Response.StatusCode, cronometro.ElapsedMilliseconds);
});

// [B] Exceções centralizadas
app.UseExceptionHandler(new ExceptionHandlerOptions
{
    StatusCodeSelector = ex => ex is BadHttpRequestException erro ? erro.StatusCode : 500
});

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("FrontEndLocal");

app.UseRateLimiter();
app.UseOutputCache();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(opcoes =>
        opcoes.SwaggerEndpoint("/openapi/v1.json", "Portal de Tarefas v1"));
}

// =============== 3) ENDPOINTS ===============
app.MapControllerRoute("default", "{controller=Tarefas}/{action=Index}/{id?}");

app.MapContaEndpoints();
app.MapExternoEndpoints();
app.MapTarefaEndpoints();
app.MapPainelEndpoints();
app.MapControllers();

// Roteiro 8: agora mostra também a VERSÃO (o commit que gerou a imagem)
app.MapGet("/api/diagnostico", (IWebHostEnvironment ambiente, IConfiguration configuracao) => Results.Ok(new
{
    abordagem = "Minimal API",
    ambiente = ambiente.EnvironmentName,
    versao = configuracao["APP_VERSAO"] ?? "local",
    horario = DateTime.Now
})).ExcludeFromDescription();

app.MapGet("/api/diagnostico/erro", () =>
{
    throw new InvalidOperationException("Erro proposital para teste");
}).ExcludeFromDescription();

// Roteiro 6: seed CONTROLADO — só roda com: dotnet run --launch-profile http -- --seed
if (args.Contains("--seed"))
    await SeedIdentidade.ExecutarAsync(app);

// Roteiro 7: HEALTH CHECKS
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = verificacao => verificacao.Tags.Contains("ready") });

// Roteiro 8: dentro do contêiner não há "dotnet ef database update". Com Banco:AplicarMigrations = true,
// a própria aplicação cria ou atualiza o banco ao iniciar (tabelas + seed das migrations).
if (app.Configuration.GetValue<bool>("Banco:AplicarMigrations"))
{
    using var escopo = app.Services.CreateScope();
    escopo.ServiceProvider.GetRequiredService<PortalTarefasDbContext>().Database.Migrate();
}

app.Run();
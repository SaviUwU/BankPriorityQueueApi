using BankPriorityQueueApi.Application.Interfaces;
using BankPriorityQueueApi.Application.Services;
using BankPriorityQueueApi.Infrastructure.Persistence;
using BankPriorityQueueApi.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------------------
// INJEÇÃO DE DEPENDÊNCIA (container de serviços)
// Tudo é registrado por INTERFACE -> as camadas dependem de abstrações (SOLID).
// ---------------------------------------------------------------------------

builder.Services.AddControllers();

// Banco de dados PostgreSQL via EF Core.
var connectionString = builder.Configuration.GetConnectionString("Default");
builder.Services.AddDbContext<AppDbContext>(opt => opt.UseNpgsql(connectionString));

// Repositório (Infrastructure) por requisição (Scoped, igual ao DbContext).
builder.Services.AddScoped<IAtendimentoRepository, AtendimentoRepository>();

// Caso de uso (Application).
builder.Services.AddScoped<IAtendimentoService, AtendimentoService>();

// Fila de prioridade EM MEMÓRIA: Singleton (uma só fila para a app toda).
builder.Services.AddSingleton<IPriorityQueueService, PriorityQueueService>();

// Swagger / OpenAPI (documentação interativa).
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BankPriorityQueueApi",
        Version = "v1",
        Description = "Fila de atendimento bancário com prioridade baseada em Heap. " +
                      "Regra: menor urgência (1=máxima) primeiro; empate = quem chegou antes."
    });
    // Inclui os comentários XML (resumos dos endpoints) no Swagger.
    var xml = Path.Combine(AppContext.BaseDirectory, "Api.xml");
    if (File.Exists(xml)) c.IncludeXmlComments(xml);
});

var app = builder.Build();

// ---------------------------------------------------------------------------
// STARTUP: prepara o banco e RECONSTRÓI a fila de prioridade a partir dele.
// ---------------------------------------------------------------------------
await InicializarBancoEFilaAsync(app);

// Swagger sempre ligado (facilita a demonstração em sala).
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "BankPriorityQueueApi v1");
    c.RoutePrefix = "swagger"; // disponível em /swagger
});

app.UseAuthorization();
app.MapControllers();

app.Run();

// ---------------------------------------------------------------------------
// Função auxiliar: cria o schema (com retry, pois o Postgres do Docker pode
// demorar a aceitar conexões) e carrega os "Aguardando" no Heap.
// ---------------------------------------------------------------------------
static async Task InicializarBancoEFilaAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var fila = scope.ServiceProvider.GetRequiredService<IPriorityQueueService>();
    var repo = scope.ServiceProvider.GetRequiredService<IAtendimentoRepository>();

    // Retry: espera o banco ficar pronto (até ~30s).
    for (int tentativa = 1; tentativa <= 10; tentativa++)
    {
        try
        {
            await db.Database.EnsureCreatedAsync();
            break;
        }
        catch when (tentativa < 10)
        {
            await Task.Delay(3000);
        }
    }

    // Reconstrói a fila com o que já estava aguardando no banco.
    var aguardando = await repo.ObterAguardandoAsync();
    fila.Reconstruir(aguardando);
}

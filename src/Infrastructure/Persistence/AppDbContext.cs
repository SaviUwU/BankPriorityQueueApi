using BankPriorityQueueApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankPriorityQueueApi.Infrastructure.Persistence;

/// <summary>
/// Contexto do Entity Framework Core: a ponte entre as entidades de domínio e o
/// banco PostgreSQL. Cada <see cref="DbSet{TEntity}"/> vira uma tabela.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Atendimento> Atendimentos => Set<Atendimento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Aplica todas as configurações (Fluent API) deste assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}

using BankPriorityQueueApi.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BankPriorityQueueApi.Infrastructure.Persistence;

/// <summary>
/// Mapeamento objeto-relacional do <see cref="Atendimento"/> (Fluent API).
/// Mantém as regras de schema fora da entidade de domínio (separação de camadas).
/// </summary>
public class AtendimentoConfiguration : IEntityTypeConfiguration<Atendimento>
{
    public void Configure(EntityTypeBuilder<Atendimento> builder)
    {
        builder.ToTable("atendimentos");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.NomeCliente).IsRequired().HasMaxLength(150);
        builder.Property(a => a.Cpf).IsRequired().HasMaxLength(11);
        builder.Property(a => a.TipoServico).IsRequired().HasMaxLength(100);
        builder.Property(a => a.Descricao).HasMaxLength(500);
        builder.Property(a => a.Urgencia).IsRequired();

        // Enum gravado como texto (legível direto no banco).
        builder.Property(a => a.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

        builder.Property(a => a.DataChegada).IsRequired();
        builder.Property(a => a.DataAtualizacao);

        // Índices que aceleram as buscas e a ordenação da fila.
        builder.HasIndex(a => a.Cpf);
        builder.HasIndex(a => new { a.Status, a.Urgencia, a.DataChegada });
    }
}

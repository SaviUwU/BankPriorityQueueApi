using BankPriorityQueueApi.Domain.Entities;

namespace BankPriorityQueueApi.Application.Interfaces;

/// <summary>
/// Contrato de persistência de atendimentos.
/// Definido na Application; IMPLEMENTADO na Infrastructure (EF Core/Postgres).
/// Assim o caso de uso não conhece o banco -> Inversão de Dependência (SOLID).
/// </summary>
public interface IAtendimentoRepository
{
    Task<Atendimento?> ObterPorIdAsync(Guid id);

    Task AdicionarAsync(Atendimento atendimento);

    Task AtualizarAsync(Atendimento atendimento);

    /// <summary>Lista atendimentos ATIVOS (status Aguardando) já paginados e ordenados por prioridade.</summary>
    Task<(IReadOnlyList<Atendimento> Itens, int Total)> ListarAtivosAsync(int pagina, int tamanho);

    /// <summary>Busca por CPF (exato, só dígitos) e/ou parte da descrição.</summary>
    Task<IReadOnlyList<Atendimento>> BuscarAsync(string? cpf, string? descricao);

    /// <summary>Todos os atendimentos aguardando — usado para RECONSTRUIR o Heap no startup.</summary>
    Task<IReadOnlyList<Atendimento>> ObterAguardandoAsync();
}

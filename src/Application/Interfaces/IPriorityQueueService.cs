using BankPriorityQueueApi.Domain.Entities;

namespace BankPriorityQueueApi.Application.Interfaces;

/// <summary>
/// Fila de prioridade EM MEMÓRIA (espelha os atendimentos "Aguardando" do banco).
/// Encapsula o uso do Heap para que o resto do sistema não dependa da estrutura.
/// </summary>
public interface IPriorityQueueService
{
    /// <summary>Insere um atendimento na fila. O(log n).</summary>
    void Enfileirar(Atendimento atendimento);

    /// <summary>Remove um atendimento da fila (cancelamento/atualização).</summary>
    void Remover(Atendimento atendimento);

    /// <summary>Reposiciona um atendimento após mudança de urgência (remove + reinsere).</summary>
    void Atualizar(Atendimento atendimento);

    /// <summary>Mostra o próximo a ser atendido sem removê-lo. O(1).</summary>
    Atendimento? VerProximo();

    /// <summary>Remove e devolve o próximo (chama o cliente). O(log n).</summary>
    Atendimento? Atender();

    /// <summary>Reconstrói a fila a partir de uma lista (startup).</summary>
    void Reconstruir(IEnumerable<Atendimento> atendimentos);

    int Quantidade { get; }
}

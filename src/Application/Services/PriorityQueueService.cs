using BankPriorityQueueApi.Application.Interfaces;
using BankPriorityQueueApi.Domain.DataStructures;
using BankPriorityQueueApi.Domain.Entities;

namespace BankPriorityQueueApi.Application.Services;

/// <summary>
/// Implementação da fila de prioridade em memória usando o <see cref="PriorityHeap{T}"/>.
///
/// Registrada como SINGLETON: existe uma única fila para toda a aplicação, viva
/// enquanto a API roda. Por isso protegemos as operações com um lock (acesso
/// concorrente de várias requisições HTTP).
///
/// O banco continua sendo a "fonte da verdade"; este serviço é a ORDENAÇÃO rápida
/// da fila. No startup, o heap é reconstruído a partir do banco (Reconstruir).
/// </summary>
public sealed class PriorityQueueService : IPriorityQueueService
{
    private readonly PriorityHeap<Atendimento> _heap = new(PriorityComparer.Instancia);
    private readonly object _lock = new();

    public int Quantidade
    {
        get { lock (_lock) return _heap.Count; }
    }

    public void Enfileirar(Atendimento atendimento)
    {
        lock (_lock) _heap.Enqueue(atendimento);
    }

    public void Remover(Atendimento atendimento)
    {
        lock (_lock) _heap.Remove(atendimento);
    }

    public void Atualizar(Atendimento atendimento)
    {
        // Remove a versão antiga e reinsere com a urgência nova -> reposiciona no heap.
        lock (_lock)
        {
            _heap.Remove(atendimento);
            _heap.Enqueue(atendimento);
        }
    }

    public Atendimento? VerProximo()
    {
        lock (_lock) return _heap.IsEmpty ? null : _heap.Peek();
    }

    public Atendimento? Atender()
    {
        lock (_lock) return _heap.IsEmpty ? null : _heap.Dequeue();
    }

    public void Reconstruir(IEnumerable<Atendimento> atendimentos)
    {
        lock (_lock)
        {
            _heap.Clear();
            foreach (var a in atendimentos)
                _heap.Enqueue(a);
        }
    }
}

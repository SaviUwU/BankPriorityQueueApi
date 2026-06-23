namespace BankPriorityQueueApi.Domain.Enums;

/// <summary>
/// Status do atendimento na fila.
/// É a base da EXCLUSÃO LÓGICA: o DELETE não apaga a linha no banco,
/// apenas muda o status para <see cref="Cancelado"/>.
/// </summary>
public enum StatusAtendimento
{
    /// <summary>Cliente aguardando na fila (entra no Heap).</summary>
    Aguardando = 0,

    /// <summary>Cliente sendo atendido (saiu do topo da fila).</summary>
    EmAtendimento = 1,

    /// <summary>Atendimento concluído.</summary>
    Finalizado = 2,

    /// <summary>Excluído logicamente (DELETE). Continua no banco para histórico.</summary>
    Cancelado = 3
}

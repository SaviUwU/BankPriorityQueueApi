using BankPriorityQueueApi.Domain.Entities;

namespace BankPriorityQueueApi.Application.Services;

/// <summary>
/// REGRA DE PRIORIDADE — explícita e automatizada (com tratamento de empate).
///
/// Define a ordem da fila comparando dois atendimentos:
///   1) MENOR Urgencia vence  (1 = máxima é atendida antes de 5 = mínima).
///   2) EMPATE de urgência -> MENOR DataChegada vence (quem chegou primeiro).
///
/// Como DataChegada é um timestamp em UTC com precisão de ticks, o empate real é
/// praticamente impossível -> a ordenação é DETERMINÍSTICA. Como critério final de
/// desempate absoluto, usamos o Id (estável) para nunca haver ambiguidade.
///
/// Este comparador alimenta o <c>PriorityHeap</c>: Compare(a,b) &lt; 0 => "a" fica
/// mais perto do topo (é atendido antes).
/// </summary>
public sealed class PriorityComparer : IComparer<Atendimento>
{
    public static readonly PriorityComparer Instancia = new();

    public int Compare(Atendimento? a, Atendimento? b)
    {
        if (a is null || b is null)
            throw new ArgumentNullException("Atendimento nulo na comparação de prioridade.");

        // 1) Urgência: menor número = mais prioritário.
        int porUrgencia = a.Urgencia.CompareTo(b.Urgencia);
        if (porUrgencia != 0) return porUrgencia;

        // 2) Desempate: quem chegou primeiro.
        int porChegada = a.DataChegada.CompareTo(b.DataChegada);
        if (porChegada != 0) return porChegada;

        // 3) Desempate final absoluto (estabilidade total).
        return a.Id.CompareTo(b.Id);
    }
}

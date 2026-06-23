using System.Reflection;
using BankPriorityQueueApi.Application.Services;
using BankPriorityQueueApi.Domain.DataStructures;
using BankPriorityQueueApi.Domain.Entities;
using Xunit;

namespace BankPriorityQueueApi.Tests;

/// <summary>
/// Testa a REGRA DE PRIORIDADE (PriorityComparer) e sua integração com o Heap:
///   1) menor urgência é atendida primeiro;
///   2) empate de urgência -> quem chegou primeiro.
/// </summary>
public class PriorityComparerTests
{
    // CPF de teste válido (passa nos dígitos verificadores).
    private const string CpfValido = "529.982.247-25";

    private static Atendimento NovoAtendimento(int urgencia, DateTime chegada)
    {
        var a = Atendimento.Criar("Cliente", CpfValido, "Saque", "teste", urgencia);
        // DataChegada tem setter privado; ajustamos via reflexão só para o teste,
        // garantindo cenários determinísticos de desempate.
        typeof(Atendimento).GetProperty("DataChegada")!
            .SetValue(a, chegada);
        return a;
    }

    [Fact]
    public void MenorUrgencia_TemPrioridade()
    {
        var urgente = NovoAtendimento(1, DateTime.UtcNow);
        var tranquilo = NovoAtendimento(5, DateTime.UtcNow);

        // Compare < 0 => 'urgente' fica mais perto do topo.
        Assert.True(PriorityComparer.Instancia.Compare(urgente, tranquilo) < 0);
    }

    [Fact]
    public void MesmaUrgencia_QuemChegouPrimeiroVence()
    {
        var agora = DateTime.UtcNow;
        var cedo = NovoAtendimento(2, agora.AddMinutes(-10));
        var tarde = NovoAtendimento(2, agora);

        Assert.True(PriorityComparer.Instancia.Compare(cedo, tarde) < 0);
    }

    [Fact]
    public void Heap_RespeitaRegraDePrioridade()
    {
        var agora = DateTime.UtcNow;
        var heap = new PriorityHeap<Atendimento>(PriorityComparer.Instancia);

        var normalAntigo = NovoAtendimento(3, agora.AddMinutes(-30)); // chegou cedo, urgência média
        var urgenteNovo = NovoAtendimento(1, agora);                  // chegou agora, urgência máxima
        var normalNovo = NovoAtendimento(3, agora.AddMinutes(-5));    // urgência média, chegou depois

        heap.Enqueue(normalAntigo);
        heap.Enqueue(urgenteNovo);
        heap.Enqueue(normalNovo);

        // Ordem esperada: urgente (1) -> normalAntigo (3, chegou antes) -> normalNovo (3).
        Assert.Equal(urgenteNovo.Id, heap.Dequeue().Id);
        Assert.Equal(normalAntigo.Id, heap.Dequeue().Id);
        Assert.Equal(normalNovo.Id, heap.Dequeue().Id);
    }
}

using BankPriorityQueueApi.Domain.DataStructures;
using Xunit;

namespace BankPriorityQueueApi.Tests;

/// <summary>
/// Testa a MECÂNICA do Heap (independente de regra de negócio): com um min-heap
/// de inteiros, o Dequeue deve sempre devolver o menor valor restante.
/// </summary>
public class PriorityHeapTests
{
    [Fact]
    public void Dequeue_DevolveSempreOMenor()
    {
        var heap = new PriorityHeap<int>(Comparer<int>.Default);
        foreach (var n in new[] { 5, 1, 8, 3, 9, 2, 7 })
            heap.Enqueue(n);

        var saida = new List<int>();
        while (!heap.IsEmpty)
            saida.Add(heap.Dequeue());

        Assert.Equal(new[] { 1, 2, 3, 5, 7, 8, 9 }, saida);
    }

    [Fact]
    public void Peek_NaoRemove()
    {
        var heap = new PriorityHeap<int>(Comparer<int>.Default);
        heap.Enqueue(10);
        heap.Enqueue(4);

        Assert.Equal(4, heap.Peek());
        Assert.Equal(2, heap.Count); // continua com 2
    }

    [Fact]
    public void Remove_TiraElementoDoMeioEReequilibra()
    {
        var heap = new PriorityHeap<int>(Comparer<int>.Default);
        foreach (var n in new[] { 1, 2, 3, 4, 5 })
            heap.Enqueue(n);

        Assert.True(heap.Remove(3));
        Assert.False(heap.Remove(99)); // inexistente

        var saida = new List<int>();
        while (!heap.IsEmpty)
            saida.Add(heap.Dequeue());

        Assert.Equal(new[] { 1, 2, 4, 5 }, saida);
    }

    [Fact]
    public void Dequeue_FilaVazia_LancaExcecao()
    {
        var heap = new PriorityHeap<int>(Comparer<int>.Default);
        Assert.Throws<InvalidOperationException>(() => heap.Dequeue());
    }
}

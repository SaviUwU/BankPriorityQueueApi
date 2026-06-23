namespace BankPriorityQueueApi.Domain.DataStructures;

/// <summary>
/// FILA DE PRIORIDADE implementada como MIN-HEAP BINÁRIO sobre um array (List).
///
/// === JUSTIFICATIVA DA ESTRUTURA DE DADOS (ponto-chave da apresentação) ===
/// Uma fila de prioridade precisa devolver SEMPRE o elemento de maior prioridade
/// (aqui: menor urgência / quem chegou primeiro) sem ter que ordenar todo mundo
/// a cada operação. O Heap binário entrega exatamente isso:
///
///   - Peek (ver o próximo):        O(1)
///   - Enqueue (inserir):           O(log n)   -> "sobe" o elemento (sift-up)
///   - Dequeue (remover o próximo): O(log n)   -> "desce" o elemento (sift-down)
///
/// Comparações:
///   - Lista ordenada: inserir custa O(n) (precisa achar a posição e deslocar).
///   - Busca linear do menor: O(n) a cada atendimento.
///   - Heap: O(log n) por inserção/remoção -> escala muito melhor.
///
/// Propriedade do heap: o pai é SEMPRE "menor ou igual" (mais prioritário) que os
/// filhos. Com array, o filho do índice i fica em 2i+1 e 2i+2; o pai de i em (i-1)/2.
/// </summary>
/// <typeparam name="T">Tipo dos elementos da fila.</typeparam>
public class PriorityHeap<T>
{
    private readonly List<T> _itens = new();
    private readonly IComparer<T> _comparador;

    /// <param name="comparador">
    /// Define a prioridade. Compare(a,b) &lt; 0 significa que "a" é MAIS prioritário
    /// que "b" e deve ficar mais perto do topo.
    /// </param>
    public PriorityHeap(IComparer<T> comparador) => _comparador = comparador;

    /// <summary>Quantidade de elementos na fila.</summary>
    public int Count => _itens.Count;

    public bool IsEmpty => _itens.Count == 0;

    /// <summary>Insere um elemento e restaura a propriedade do heap subindo-o. O(log n).</summary>
    public void Enqueue(T item)
    {
        _itens.Add(item);          // adiciona no fim
        SiftUp(_itens.Count - 1);  // sobe até o lugar certo
    }

    /// <summary>Olha o próximo (mais prioritário) sem remover. O(1).</summary>
    public T Peek()
    {
        if (IsEmpty) throw new InvalidOperationException("A fila está vazia.");
        return _itens[0]; // o topo do heap é sempre o índice 0
    }

    /// <summary>Remove e devolve o mais prioritário. O(log n).</summary>
    public T Dequeue()
    {
        if (IsEmpty) throw new InvalidOperationException("A fila está vazia.");

        T topo = _itens[0];
        int ultimo = _itens.Count - 1;

        // Move o último para o topo e remove o fim...
        _itens[0] = _itens[ultimo];
        _itens.RemoveAt(ultimo);

        // ...e "desce" esse elemento até restaurar a ordem.
        if (!IsEmpty) SiftDown(0);

        return topo;
    }

    /// <summary>
    /// Remove um elemento específico (usado quando um atendimento é cancelado ou
    /// atualizado). Como não está necessariamente no topo, fazemos remoção O(n) para
    /// localizar e depois reequilibramos a posição afetada.
    /// </summary>
    public bool Remove(T item)
    {
        int idx = _itens.IndexOf(item);
        if (idx < 0) return false;

        int ultimo = _itens.Count - 1;
        _itens[idx] = _itens[ultimo];
        _itens.RemoveAt(ultimo);

        if (idx < _itens.Count)
        {
            // O elemento trocado pode precisar subir OU descer.
            SiftUp(idx);
            SiftDown(idx);
        }
        return true;
    }

    /// <summary>Esvazia a fila (usado ao reconstruir o heap a partir do banco).</summary>
    public void Clear() => _itens.Clear();

    /// <summary>Snapshot ordenado por prioridade (NÃO consome a fila). Útil para depurar/listar.</summary>
    public IReadOnlyList<T> ToOrderedList()
    {
        var copia = new List<T>(_itens);
        copia.Sort(_comparador);
        return copia;
    }

    // ---- Operações internas que mantêm a propriedade do heap ----

    /// <summary>Sobe o elemento enquanto ele for mais prioritário que o pai.</summary>
    private void SiftUp(int i)
    {
        while (i > 0)
        {
            int pai = (i - 1) / 2;
            if (_comparador.Compare(_itens[i], _itens[pai]) >= 0) break; // pai já é melhor/igual
            Trocar(i, pai);
            i = pai;
        }
    }

    /// <summary>Desce o elemento trocando-o sempre com o filho mais prioritário.</summary>
    private void SiftDown(int i)
    {
        int n = _itens.Count;
        while (true)
        {
            int menor = i;
            int esq = 2 * i + 1;
            int dir = 2 * i + 2;

            if (esq < n && _comparador.Compare(_itens[esq], _itens[menor]) < 0) menor = esq;
            if (dir < n && _comparador.Compare(_itens[dir], _itens[menor]) < 0) menor = dir;

            if (menor == i) break; // já está no lugar
            Trocar(i, menor);
            i = menor;
        }
    }

    private void Trocar(int a, int b) => (_itens[a], _itens[b]) = (_itens[b], _itens[a]);
}

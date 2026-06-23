namespace BankPriorityQueueApi.Application.Common;

/// <summary>
/// Resultado paginado genérico devolvido pela listagem.
/// Carrega os itens da página + metadados para o cliente saber navegar.
/// </summary>
public class PagedResult<T>
{
    public IReadOnlyList<T> Itens { get; init; } = Array.Empty<T>();
    public int Pagina { get; init; }
    public int Tamanho { get; init; }
    public int Total { get; init; }
    public int TotalPaginas => Tamanho == 0 ? 0 : (int)Math.Ceiling(Total / (double)Tamanho);
}

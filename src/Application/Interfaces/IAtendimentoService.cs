using BankPriorityQueueApi.Application.Common;
using BankPriorityQueueApi.Application.DTOs;

namespace BankPriorityQueueApi.Application.Interfaces;

/// <summary>
/// Casos de uso de atendimento. O Controller depende DESTA interface, não da
/// implementação concreta (Single Responsibility + Dependency Inversion).
/// </summary>
public interface IAtendimentoService
{
    Task<AtendimentoResponseDto> CriarAsync(CreateAtendimentoDto dto);
    Task<AtendimentoResponseDto?> ObterAsync(Guid id);
    Task<PagedResult<AtendimentoResponseDto>> ListarAtivosAsync(int pagina, int tamanho);
    Task<IReadOnlyList<AtendimentoResponseDto>> BuscarAsync(string? cpf, string? descricao);
    Task<AtendimentoResponseDto?> AtualizarAsync(Guid id, UpdateAtendimentoDto dto);

    /// <summary>Exclusão LÓGICA: muda status para Cancelado. Retorna false se não achar.</summary>
    Task<bool> RemoverAsync(Guid id);

    /// <summary>Chama o próximo da fila (Heap). Bônus para a demonstração.</summary>
    Task<AtendimentoResponseDto?> AtenderProximoAsync();
}

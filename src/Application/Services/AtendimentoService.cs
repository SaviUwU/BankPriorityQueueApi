using BankPriorityQueueApi.Application.Common;
using BankPriorityQueueApi.Application.DTOs;
using BankPriorityQueueApi.Application.Interfaces;
using BankPriorityQueueApi.Domain.Entities;

namespace BankPriorityQueueApi.Application.Services;

/// <summary>
/// Caso de uso principal: orquestra o REPOSITÓRIO (persistência) e a FILA DE
/// PRIORIDADE (ordenação em memória). Mantém os dois em sincronia a cada operação.
///
/// Responsabilidade única: regras de aplicação do atendimento. Não sabe SQL nem HTTP.
/// </summary>
public sealed class AtendimentoService : IAtendimentoService
{
    private readonly IAtendimentoRepository _repositorio;
    private readonly IPriorityQueueService _fila;

    // Dependências recebidas por injeção (interfaces) -> testável e desacoplado.
    public AtendimentoService(IAtendimentoRepository repositorio, IPriorityQueueService fila)
    {
        _repositorio = repositorio;
        _fila = fila;
    }

    public async Task<AtendimentoResponseDto> CriarAsync(CreateAtendimentoDto dto)
    {
        // 1) Cria a entidade (valida CPF, urgência etc. nas invariantes do domínio).
        var atendimento = Atendimento.Criar(dto.NomeCliente, dto.Cpf, dto.TipoServico, dto.Descricao, dto.Urgencia);

        // 2) Persiste no banco.
        await _repositorio.AdicionarAsync(atendimento);

        // 3) Coloca na fila de prioridade (Heap).
        _fila.Enfileirar(atendimento);

        return AtendimentoResponseDto.FromEntity(atendimento);
    }

    public async Task<AtendimentoResponseDto?> ObterAsync(Guid id)
    {
        var a = await _repositorio.ObterPorIdAsync(id);
        return a is null ? null : AtendimentoResponseDto.FromEntity(a);
    }

    public async Task<PagedResult<AtendimentoResponseDto>> ListarAtivosAsync(int pagina, int tamanho)
    {
        // Normaliza paginação (evita valores inválidos vindos da query string).
        if (pagina < 1) pagina = 1;
        if (tamanho < 1) tamanho = 10;
        if (tamanho > 100) tamanho = 100;

        var (itens, total) = await _repositorio.ListarAtivosAsync(pagina, tamanho);

        return new PagedResult<AtendimentoResponseDto>
        {
            Itens = itens.Select(AtendimentoResponseDto.FromEntity).ToList(),
            Pagina = pagina,
            Tamanho = tamanho,
            Total = total
        };
    }

    public async Task<IReadOnlyList<AtendimentoResponseDto>> BuscarAsync(string? cpf, string? descricao)
    {
        var itens = await _repositorio.BuscarAsync(cpf, descricao);
        return itens.Select(AtendimentoResponseDto.FromEntity).ToList();
    }

    public async Task<AtendimentoResponseDto?> AtualizarAsync(Guid id, UpdateAtendimentoDto dto)
    {
        var atendimento = await _repositorio.ObterPorIdAsync(id);
        if (atendimento is null) return null;

        // Atualiza os dados (pode mudar a urgência).
        atendimento.Atualizar(dto.NomeCliente, dto.TipoServico, dto.Descricao, dto.Urgencia);
        await _repositorio.AtualizarAsync(atendimento);

        // RECALCULA a posição na fila (só faz sentido se ainda está aguardando).
        if (atendimento.Status == Domain.Enums.StatusAtendimento.Aguardando)
            _fila.Atualizar(atendimento);

        return AtendimentoResponseDto.FromEntity(atendimento);
    }

    public async Task<bool> RemoverAsync(Guid id)
    {
        var atendimento = await _repositorio.ObterPorIdAsync(id);
        if (atendimento is null) return false;

        // EXCLUSÃO LÓGICA: não apaga do banco, só muda o status.
        atendimento.Cancelar();
        await _repositorio.AtualizarAsync(atendimento);

        // Tira da fila de espera (não deve mais ser chamado).
        _fila.Remover(atendimento);
        return true;
    }

    public async Task<AtendimentoResponseDto?> AtenderProximoAsync()
    {
        // Pega o mais prioritário do Heap.
        var proximo = _fila.Atender();
        if (proximo is null) return null;

        // Reflete no banco: passa para "EmAtendimento".
        proximo.IniciarAtendimento();
        await _repositorio.AtualizarAsync(proximo);

        return AtendimentoResponseDto.FromEntity(proximo);
    }
}

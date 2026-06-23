using BankPriorityQueueApi.Application.DTOs;
using BankPriorityQueueApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BankPriorityQueueApi.Api.Controllers;

/// <summary>
/// Endpoints da fila de atendimento bancário (recurso: atendimentos-bancarios).
///
/// O controller é FINO: só recebe a requisição HTTP, chama o caso de uso
/// (IAtendimentoService) e devolve o status correto. Nenhuma regra de negócio aqui.
/// </summary>
[ApiController]
[Route("atendimentos-bancarios")]
[Produces("application/json")]
public class AtendimentosBancariosController : ControllerBase
{
    private readonly IAtendimentoService _servico;

    // Recebe o caso de uso por injeção de dependência (interface).
    public AtendimentosBancariosController(IAtendimentoService servico) => _servico = servico;

    /// <summary>POST: cadastra um atendimento, calcula a prioridade e o coloca na fila.</summary>
    [HttpPost]
    [ProducesResponseType(typeof(AtendimentoResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Criar([FromBody] CreateAtendimentoDto dto)
    {
        try
        {
            var criado = await _servico.CriarAsync(dto);
            // 201 + Location apontando para o GET por id.
            return CreatedAtAction(nameof(ObterPorId), new { id = criado.Id }, criado);
        }
        catch (ArgumentException ex)
        {
            // Ex.: CPF inválido, urgência fora de 1..5.
            return BadRequest(new { erro = ex.Message });
        }
    }

    /// <summary>GET /{id}: busca um atendimento específico.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AtendimentoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterPorId(Guid id)
    {
        var item = await _servico.ObterAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>GET: lista atendimentos ATIVOS (aguardando), paginados e em ordem de prioridade.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] int page = 1, [FromQuery] int size = 10)
    {
        var resultado = await _servico.ListarAtivosAsync(page, size);
        return Ok(resultado);
    }

    /// <summary>GET /buscar: busca por CPF (exato) e/ou trecho da descrição.</summary>
    [HttpGet("buscar")]
    [ProducesResponseType(typeof(IEnumerable<AtendimentoResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Buscar([FromQuery] string? cpf, [FromQuery] string? descricao)
    {
        if (string.IsNullOrWhiteSpace(cpf) && string.IsNullOrWhiteSpace(descricao))
            return BadRequest(new { erro = "Informe ao menos 'cpf' ou 'descricao'." });

        var itens = await _servico.BuscarAsync(cpf, descricao);
        return Ok(itens);
    }

    /// <summary>PUT /{id}: atualiza os dados e RECALCULA a prioridade na fila.</summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(AtendimentoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Atualizar(Guid id, [FromBody] UpdateAtendimentoDto dto)
    {
        try
        {
            var atualizado = await _servico.AtualizarAsync(id, dto);
            return atualizado is null ? NotFound() : Ok(atualizado);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    /// <summary>DELETE /{id}: EXCLUSÃO LÓGICA — muda o status para Cancelado (não apaga).</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remover(Guid id)
    {
        var removido = await _servico.RemoverAsync(id);
        return removido ? NoContent() : NotFound();
    }

    /// <summary>GET /proximo: chama o próximo da fila (Heap.Dequeue). Bônus para a demo.</summary>
    [HttpGet("proximo")]
    [ProducesResponseType(typeof(AtendimentoResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Proximo()
    {
        var proximo = await _servico.AtenderProximoAsync();
        return proximo is null ? NoContent() : Ok(proximo);
    }
}

using BankPriorityQueueApi.Application.Interfaces;
using BankPriorityQueueApi.Domain.Entities;
using BankPriorityQueueApi.Domain.Enums;
using BankPriorityQueueApi.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace BankPriorityQueueApi.Infrastructure.Repositories;

/// <summary>
/// Implementação EF Core do repositório de atendimentos.
/// É AQUI (e só aqui) que existe conhecimento de banco/SQL.
/// </summary>
public class AtendimentoRepository : IAtendimentoRepository
{
    private readonly AppDbContext _db;

    public AtendimentoRepository(AppDbContext db) => _db = db;

    public async Task<Atendimento?> ObterPorIdAsync(Guid id) =>
        await _db.Atendimentos.FirstOrDefaultAsync(a => a.Id == id);

    public async Task AdicionarAsync(Atendimento atendimento)
    {
        _db.Atendimentos.Add(atendimento);
        await _db.SaveChangesAsync();
    }

    public async Task AtualizarAsync(Atendimento atendimento)
    {
        _db.Atendimentos.Update(atendimento);
        await _db.SaveChangesAsync();
    }

    public async Task<(IReadOnlyList<Atendimento> Itens, int Total)> ListarAtivosAsync(int pagina, int tamanho)
    {
        // "Ativos" = aguardando atendimento. A exclusão lógica (Cancelado) some daqui.
        var query = _db.Atendimentos
            .Where(a => a.Status == StatusAtendimento.Aguardando)
            // Mesma ordem da fila de prioridade: urgência, depois chegada.
            .OrderBy(a => a.Urgencia)
            .ThenBy(a => a.DataChegada);

        int total = await query.CountAsync();

        var itens = await query
            .Skip((pagina - 1) * tamanho)
            .Take(tamanho)
            .ToListAsync();

        return (itens, total);
    }

    public async Task<IReadOnlyList<Atendimento>> BuscarAsync(string? cpf, string? descricao)
    {
        // Não mostra cancelados (coerente com a exclusão lógica).
        var query = _db.Atendimentos.Where(a => a.Status != StatusAtendimento.Cancelado);

        if (!string.IsNullOrWhiteSpace(cpf))
        {
            var digitos = Regex.Replace(cpf, "[^0-9]", "");
            query = query.Where(a => a.Cpf == digitos);
        }

        if (!string.IsNullOrWhiteSpace(descricao))
        {
            // ILike = busca case-insensitive do PostgreSQL.
            var termo = $"%{descricao}%";
            query = query.Where(a => EF.Functions.ILike(a.Descricao, termo));
        }

        return await query
            .OrderBy(a => a.Urgencia)
            .ThenBy(a => a.DataChegada)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Atendimento>> ObterAguardandoAsync() =>
        await _db.Atendimentos
            .Where(a => a.Status == StatusAtendimento.Aguardando)
            .ToListAsync();
}

using BankPriorityQueueApi.Domain.Entities;
using BankPriorityQueueApi.Domain.ValueObjects;

namespace BankPriorityQueueApi.Application.DTOs;

/// <summary>Formato de saída (resposta) de um atendimento.</summary>
public class AtendimentoResponseDto
{
    public Guid Id { get; set; }
    public string NomeCliente { get; set; } = string.Empty;
    public string Cpf { get; set; } = string.Empty;
    public string TipoServico { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public int Urgencia { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime DataChegada { get; set; }
    public DateTime? DataAtualizacao { get; set; }

    /// <summary>Mapeia a entidade de domínio para o DTO de resposta.</summary>
    public static AtendimentoResponseDto FromEntity(Atendimento a) => new()
    {
        Id = a.Id,
        NomeCliente = a.NomeCliente,
        Cpf = BankPriorityQueueApi.Domain.ValueObjects.Cpf.Criar(a.Cpf).Formatado(),
        TipoServico = a.TipoServico,
        Descricao = a.Descricao,
        Urgencia = a.Urgencia,
        Status = a.Status.ToString(),
        DataChegada = a.DataChegada,
        DataAtualizacao = a.DataAtualizacao
    };
}

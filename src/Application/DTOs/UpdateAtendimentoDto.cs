using System.ComponentModel.DataAnnotations;

namespace BankPriorityQueueApi.Application.DTOs;

/// <summary>Dados de entrada do PUT (atualização + recálculo de prioridade).</summary>
public class UpdateAtendimentoDto
{
    [Required] public string NomeCliente { get; set; } = string.Empty;
    [Required] public string TipoServico { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    [Range(1, 5)] public int Urgencia { get; set; }
}

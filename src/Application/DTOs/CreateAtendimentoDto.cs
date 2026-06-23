using System.ComponentModel.DataAnnotations;

namespace BankPriorityQueueApi.Application.DTOs;

/// <summary>Dados de entrada do POST (cadastro de atendimento).</summary>
public class CreateAtendimentoDto
{
    [Required] public string NomeCliente { get; set; } = string.Empty;
    [Required] public string Cpf { get; set; } = string.Empty;
    [Required] public string TipoServico { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;

    /// <summary>1 = urgência máxima ... 5 = urgência mínima.</summary>
    [Range(1, 5)] public int Urgencia { get; set; }
}

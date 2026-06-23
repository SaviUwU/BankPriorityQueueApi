using BankPriorityQueueApi.Domain.Enums;

namespace BankPriorityQueueApi.Domain.Entities;

/// <summary>
/// Entidade central do domínio: um atendimento bancário na fila de prioridade.
///
/// É uma ENTIDADE (DDD) porque tem identidade própria (<see cref="Id"/>): dois
/// atendimentos com os mesmos dados ainda são pessoas diferentes na fila.
/// </summary>
public class Atendimento
{
    /// <summary>Identificador único (chave primária no banco).</summary>
    public Guid Id { get; private set; }

    public string NomeCliente { get; private set; } = string.Empty;

    /// <summary>CPF armazenado só com dígitos. Validado via Value Object Cpf na criação.</summary>
    public string Cpf { get; private set; } = string.Empty;

    /// <summary>Tipo de serviço (ex.: Saque, Depósito, Empréstimo).</summary>
    public string TipoServico { get; private set; } = string.Empty;

    /// <summary>Descrição livre — usada na busca textual.</summary>
    public string Descricao { get; private set; } = string.Empty;

    /// <summary>
    /// Nível de urgência informado no cadastro: 1 = MÁXIMA ... 5 = MÍNIMA.
    /// É o critério PRINCIPAL de prioridade na fila (Heap).
    /// </summary>
    public int Urgencia { get; private set; }

    /// <summary>Status atual. Suporta a exclusão lógica (Cancelado).</summary>
    public StatusAtendimento Status { get; private set; }

    /// <summary>
    /// Momento de chegada (UTC). É o critério de DESEMPATE: mesma urgência,
    /// quem chegou primeiro é atendido primeiro (FIFO dentro do nível).
    /// </summary>
    public DateTime DataChegada { get; private set; }

    public DateTime? DataAtualizacao { get; private set; }

    // Construtor sem parâmetros exigido pelo EF Core para materializar do banco.
    private Atendimento() { }

    /// <summary>
    /// Fábrica de criação. Centraliza as invariantes (urgência válida, CPF válido,
    /// status inicial = Aguardando, data de chegada = agora).
    /// </summary>
    public static Atendimento Criar(
        string nomeCliente,
        string cpf,
        string tipoServico,
        string descricao,
        int urgencia)
    {
        ValidarUrgencia(urgencia);

        // Valida o CPF via Value Object; guarda só os dígitos.
        var cpfValido = ValueObjects.Cpf.Criar(cpf);

        return new Atendimento
        {
            Id = Guid.NewGuid(),
            NomeCliente = ExigirTexto(nomeCliente, nameof(nomeCliente)),
            Cpf = cpfValido.Numero,
            TipoServico = ExigirTexto(tipoServico, nameof(tipoServico)),
            Descricao = descricao?.Trim() ?? string.Empty,
            Urgencia = urgencia,
            Status = StatusAtendimento.Aguardando,
            DataChegada = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Atualiza dados editáveis. Usado no PUT. Ao mudar a urgência, a posição
    /// na fila precisa ser RECALCULADA (re-heapify) pela camada de serviço.
    /// </summary>
    public void Atualizar(string nomeCliente, string tipoServico, string descricao, int urgencia)
    {
        ValidarUrgencia(urgencia);
        NomeCliente = ExigirTexto(nomeCliente, nameof(nomeCliente));
        TipoServico = ExigirTexto(tipoServico, nameof(tipoServico));
        Descricao = descricao?.Trim() ?? string.Empty;
        Urgencia = urgencia;
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>EXCLUSÃO LÓGICA: muda o status para Cancelado, sem remover do banco.</summary>
    public void Cancelar()
    {
        Status = StatusAtendimento.Cancelado;
        DataAtualizacao = DateTime.UtcNow;
    }

    /// <summary>Marca como em atendimento (quando sai do topo da fila).</summary>
    public void IniciarAtendimento()
    {
        Status = StatusAtendimento.EmAtendimento;
        DataAtualizacao = DateTime.UtcNow;
    }

    private static void ValidarUrgencia(int urgencia)
    {
        if (urgencia is < 1 or > 5)
            throw new ArgumentException("Urgência deve estar entre 1 (máxima) e 5 (mínima).");
    }

    private static string ExigirTexto(string valor, string campo)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException($"Campo '{campo}' é obrigatório.");
        return valor.Trim();
    }
    // === Igualdade por identidade (Id) ===
    // Necessária para o Heap localizar/remover o atendimento certo mesmo quando o
    // EF Core devolve uma instância diferente em outro escopo. Entidade = identidade.
    public override bool Equals(object? obj) => obj is Atendimento outro && outro.Id == Id;
    public override int GetHashCode() => Id.GetHashCode();
}


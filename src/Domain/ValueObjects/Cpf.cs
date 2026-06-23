using System.Text.RegularExpressions;

namespace BankPriorityQueueApi.Domain.ValueObjects;

/// <summary>
/// Value Object (DDD) que representa um CPF válido.
///
/// Por que um Value Object? Porque um CPF não é "qualquer string": ele tem
/// regras de validade (11 dígitos + dígitos verificadores). Encapsular isso
/// num tipo garante que, se existe um <see cref="Cpf"/>, ele JÁ é válido.
/// Não tem identidade própria — dois CPFs com o mesmo número são iguais.
/// </summary>
public sealed class Cpf : IEquatable<Cpf>
{
    /// <summary>CPF armazenado só com dígitos (sem pontos/traços).</summary>
    public string Numero { get; }

    private Cpf(string numero) => Numero = numero;

    /// <summary>
    /// Cria um CPF validado. Lança <see cref="ArgumentException"/> se inválido.
    /// </summary>
    public static Cpf Criar(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
            throw new ArgumentException("CPF é obrigatório.");

        // Remove tudo que não for dígito (aceita "123.456.789-09" ou "12345678909").
        var digitos = Regex.Replace(valor, "[^0-9]", "");

        if (!EhValido(digitos))
            throw new ArgumentException($"CPF inválido: {valor}");

        return new Cpf(digitos);
    }

    /// <summary>Validação clássica de CPF com os dois dígitos verificadores.</summary>
    private static bool EhValido(string cpf)
    {
        if (cpf.Length != 11) return false;

        // Rejeita sequências repetidas (ex.: 111.111.111-11), que passam no cálculo mas são inválidas.
        if (cpf.Distinct().Count() == 1) return false;

        // Primeiro dígito verificador.
        int soma = 0;
        for (int i = 0; i < 9; i++)
            soma += (cpf[i] - '0') * (10 - i);
        int dig1 = CalcularDigito(soma);
        if (dig1 != cpf[9] - '0') return false;

        // Segundo dígito verificador.
        soma = 0;
        for (int i = 0; i < 10; i++)
            soma += (cpf[i] - '0') * (11 - i);
        int dig2 = CalcularDigito(soma);
        return dig2 == cpf[10] - '0';
    }

    private static int CalcularDigito(int soma)
    {
        int resto = soma % 11;
        return resto < 2 ? 0 : 11 - resto;
    }

    /// <summary>Formata como 000.000.000-00 para exibição.</summary>
    public string Formatado() =>
        Convert.ToUInt64(Numero).ToString(@"000\.000\.000\-00");

    // Igualdade por valor (característica de Value Object).
    public bool Equals(Cpf? other) => other is not null && Numero == other.Numero;
    public override bool Equals(object? obj) => Equals(obj as Cpf);
    public override int GetHashCode() => Numero.GetHashCode();
    public override string ToString() => Numero;
}

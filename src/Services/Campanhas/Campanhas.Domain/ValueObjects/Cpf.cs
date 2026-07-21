using Campanhas.Domain.Exceptions;

namespace Campanhas.Domain.ValueObjects;

/// <summary>
/// Value object de CPF: normaliza (remove máscara), valida os dígitos
/// verificadores (módulo 11) e expõe a forma mascarada para saídas (LGPD).
/// </summary>
// SOLID (SRP): toda a regra de CPF vive aqui — nenhum controller ou serviço
// conhece o algoritmo de validação.
public sealed record Cpf
{
    public string Valor { get; }

    private Cpf(string valor) => Valor = valor;

    public static Cpf Criar(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            throw new RegraDeNegocioException("CPF é obrigatório.");

        var digitos = new string(cpf.Where(char.IsDigit).ToArray());

        if (digitos.Length != 11)
            throw new RegraDeNegocioException("CPF deve conter 11 dígitos.");

        if (digitos.Distinct().Count() == 1)
            throw new RegraDeNegocioException("CPF inválido.");

        if (!DigitosVerificadoresValidos(digitos))
            throw new RegraDeNegocioException("CPF inválido.");

        return new Cpf(digitos);
    }

    private static bool DigitosVerificadoresValidos(string digitos)
    {
        var numeros = digitos.Select(c => c - '0').ToArray();

        var soma1 = 0;
        for (var i = 0; i < 9; i++)
            soma1 += numeros[i] * (10 - i);
        var dv1 = soma1 % 11 < 2 ? 0 : 11 - soma1 % 11;
        if (numeros[9] != dv1) return false;

        var soma2 = 0;
        for (var i = 0; i < 10; i++)
            soma2 += numeros[i] * (11 - i);
        var dv2 = soma2 % 11 < 2 ? 0 : 11 - soma2 % 11;
        return numeros[10] == dv2;
    }

    /// <summary>Forma segura para exibição: 123.***.***-45 (minimização LGPD).</summary>
    public string Mascarado() => $"{Valor[..3]}.***.***-{Valor[9..]}";

    public override string ToString() => Valor;
}

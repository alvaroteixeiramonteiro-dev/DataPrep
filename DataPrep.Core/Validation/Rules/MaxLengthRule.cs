using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DataPrep.Core.Validation.Rules;

/// <summary>
/// Verifica se um texto ultrapassa o comprimento máximo configurado.
/// Não modifica nem corta o valor original.
/// </summary>
public sealed class MaxLengthRule
{
    /// <summary>
    /// Devolve um diagnóstico se o texto ultrapassar o limite,
    /// ou null quando não existir uma infração desta regra.
    /// </summary>
    /// <remarks>
    /// Utiliza string.Length, que conta unidades UTF-16.
    /// Alguns símbolos, como determinados emojis, ocupam duas unidades.
    /// A compatibilidade com os limites do destino SQL será tratada
    /// quando for implementada a validação do esquema da base de dados.
    /// </remarks>
    /// <param name="value">Valor original do campo.</param>
    /// <param name="maxLength">Comprimento máximo permitido, incluindo zero.</param>
    /// <param name="sourceField">Nome do campo de origem.</param>
    /// <param name="targetField">Nome do campo de destino.</param>
    /// <param name="recordNumber">Número do registo, excluindo o cabeçalho.</param>
    /// <param name="startLineNumber">Primeira linha física do registo.</param>
    /// <param name="endLineNumber">Última linha física do registo.</param>
    /// <param name="columnNumber">Posição da coluna, começando em 1.</param>
    public ValidationIssue? Validate(
        string? value,
        int maxLength,
        string sourceField,
        string? targetField = null,
        long? recordNumber = null,
        long? startLineNumber = null,
        long? endLineNumber = null,
        int? columnNumber = null)
    {
        // Um limite negativo representa uma configuração inválida
        // da regra, e não um problema nos dados recebidos.
        ArgumentOutOfRangeException.ThrowIfNegative(maxLength);

        // A ausência de valor pertence à regra de obrigatoriedade.
        // Um texto com comprimento igual ao limite também é aceite.
        if (value is null || value.Length <= maxLength)
        {
            return null;
        }

        // Os parâmetros usam uma representação independente do idioma.
        // A interface poderá utilizá-los nas mensagens traduzidas.
        return new ValidationIssue
        {
            RuleCode = "validation.max_length",
            Severity = ValidationSeverity.Error,
            RecordNumber = recordNumber,
            StartLineNumber = startLineNumber,
            EndLineNumber = endLineNumber,
            ColumnNumber = columnNumber,
            SourceField = sourceField,
            TargetField = targetField,
            OriginalValue = value,
            CorrectionCode = "correction.shorten_value",
            Parameters = new Dictionary<string, string>
            {
                ["maxLength"] = maxLength.ToString(
                    CultureInfo.InvariantCulture),

                ["actualLength"] = value.Length.ToString(
                    CultureInfo.InvariantCulture)
            }
        };
    }
}

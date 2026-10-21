using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

namespace DataPrep.Core.Validation.Rules;
/// <summary>
/// Verifica se um valor representa um número inteiro no intervalo de long.
/// A interpretação é independente do idioma da interface.
/// </summary>
public sealed class IntegerRule
{
    /// <summary>
    /// Devolve um diagnóstico quando o valor não representa um inteiro,
    /// ou null quando esta regra não encontra um problema.
    /// </summary>
    /// <param name="value">Valor original, sem transformações.</param>
    /// <param name="sourceField">Nome do campo de origem.</param>
    /// <param name="targetField">Nome do campo de destino.</param>
    /// <param name="recordNumber">Número do registo, excluindo o cabeçalho.</param>
    /// <param name="startLineNumber">Primeira linha física do registo.</param>
    /// <param name="endLineNumber">Última linha física do registo.</param>
    /// <param name="columnNumber">Posição da coluna, começando em 1.</param>
    public ValidationIssue? Validate(
        string? value,
        string sourceField,
        string? targetField = null,
        long? recordNumber = null,
        long? startLineNumber = null,
        long? endLineNumber = null,
        int? columnNumber = null)
    {
        // A falta de conteúdo é tratada pela regra de obrigatoriedade,
        // evitando produzir dois erros para o mesmo campo vazio.
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        // Permite sinal inicial, mas não espaços, separadores ou decimais.
        // O descarte "_" indica que só precisamos de saber se é válido.
        bool isValid = long.TryParse(
            value,
            NumberStyles.AllowLeadingSign,
            CultureInfo.InvariantCulture,
            out _);

        if (isValid)
        {
            return null;
        }

        // Conserva o valor e a localização para apresentar o problema.
        // Os limites permitem explicar também o intervalo aceite.
        return new ValidationIssue
        {
            RuleCode = "validation.integer",
            Severity = ValidationSeverity.Error,
            RecordNumber = recordNumber,
            StartLineNumber = startLineNumber,
            EndLineNumber = endLineNumber,
            ColumnNumber = columnNumber,
            SourceField = sourceField,
            TargetField = targetField,
            OriginalValue = value,
            CorrectionCode = "correction.provide_integer",
            Parameters = new Dictionary<string, string>
            {
                ["minValue"] = long.MinValue.ToString(
                    CultureInfo.InvariantCulture),

                ["maxValue"] = long.MaxValue.ToString(
                    CultureInfo.InvariantCulture)
            }
        };
    }
}

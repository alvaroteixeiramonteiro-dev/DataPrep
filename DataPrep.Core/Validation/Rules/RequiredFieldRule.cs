using System;
using System.Collections.Generic;
using System.Text;

namespace DataPrep.Core.Validation.Rules
{
    /// <summary>
    /// Verifica se um campo obrigatório contém um valor.
    /// Não modifica os dados nem produz mensagens dependentes de um idioma.
    /// </summary>
    public sealed class RequiredFieldRule
    {
        /// <summary>
        /// Valida o valor de um campo e devolve o diagnóstico correspondente.
        /// Devolve null quando o valor cumpre a regra.
        /// As posições começam em 1; o número do registo exclui o cabeçalho.
        /// </summary>
        /// <param name="value">Valor original do campo, antes das transformações.</param>
        /// <param name="sourceField">Nome do campo no ficheiro de origem.</param>
        /// <param name="targetField">Nome do campo de destino, quando conhecido.</param>
        /// <param name="recordNumber">Número do registo de dados.</param>
        /// <param name="startLineNumber">Primeira linha física do registo.</param>
        /// <param name="endLineNumber">Última linha física do registo.</param>
        /// <param name="columnNumber">Posição da coluna no ficheiro.</param>
        public ValidationIssue? Validate(
        string? value,
        string sourceField,
        string? targetField = null,
        long? recordNumber = null,
        long? startLineNumber = null,
        long? endLineNumber = null,
        int? columnNumber = null)
        {
            // Um campo obrigatório não aceita ausência de valor,
            // texto vazio ou texto composto apenas por espaços em branco.
            if (!string.IsNullOrWhiteSpace(value))
            {
                return null;
            }

            // Conserva o valor e a localização para permitir um diagnóstico
            // preciso. A tradução das mensagens fica a cargo da interface.
            return new ValidationIssue
            {
                RuleCode = "validation.required",
                Severity = ValidationSeverity.Error,
                RecordNumber = recordNumber,
                StartLineNumber = startLineNumber,
                EndLineNumber = endLineNumber,
                ColumnNumber = columnNumber,
                SourceField = sourceField,
                TargetField = targetField,
                OriginalValue = value,
                CorrectionCode = "correction.provide_value"
            };
        }
    }
}

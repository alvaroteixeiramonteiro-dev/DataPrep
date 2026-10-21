using System;
using System.Collections.Generic;
using System.Text;
using DataPrep.Core.Validation;
using DataPrep.Core.Validation.Rules;
using Xunit;

namespace DataPrep.Tests.Validation;

/// <summary>
/// Verifica a interpretação de inteiros e a preservação do diagnóstico.
/// </summary>
public sealed class IntegerRuleTests
{
    /// <summary>
    /// Aceita inteiros nos limites de long.
    /// Valores vazios são deixados para a regra de obrigatoriedade.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("0")]
    [InlineData("42")]
    [InlineData("-42")]
    [InlineData("+42")]
    [InlineData("-9223372036854775808")]
    [InlineData("9223372036854775807")]
    public void Validate_WhenValueIsAccepted_ReturnsNoIssue(string? value)
    {
        // Preparação.
        var rule = new IntegerRule();

        // Execução.
        var issue = rule.Validate(value, sourceField: "quantidade");

        // Verificação.
        Assert.Null(issue);
    }

    /// <summary>
    /// Rejeita formatos não permitidos e valores fora do intervalo.
    /// Confirma que o diagnóstico conserva a localização e o conteúdo.
    /// </summary>
    [Theory]
    [InlineData("abc")]
    [InlineData("12.5")]
    [InlineData("12,5")]
    [InlineData("1,000")]
    [InlineData(" 42 ")]
    [InlineData("9223372036854775808")]
    [InlineData("-9223372036854775809")]
    public void Validate_WhenValueIsInvalid_ReturnsIntegerIssue(string value)
    {
        // Preparação.
        var rule = new IntegerRule();

        // Execução.
        var issue = rule.Validate(
            value: value,
            sourceField: "quantidade",
            targetField: "quantity",
            recordNumber: 4,
            startLineNumber: 5,
            endLineNumber: 5,
            columnNumber: 3);

        // Verificação da identificação e da correção do problema.
        Assert.NotNull(issue);
        Assert.Equal("validation.integer", issue.RuleCode);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("correction.provide_integer", issue.CorrectionCode);

        // Verificação da rastreabilidade do valor rejeitado.
        Assert.Equal(value, issue.OriginalValue);
        Assert.Equal("quantidade", issue.SourceField);
        Assert.Equal("quantity", issue.TargetField);
        Assert.Equal(4L, issue.RecordNumber);
        Assert.Equal(5L, issue.StartLineNumber);
        Assert.Equal(5L, issue.EndLineNumber);
        Assert.Equal(3, issue.ColumnNumber);

        // Os limites devem manter a mesma representação em qualquer idioma.
        Assert.Equal("-9223372036854775808", issue.Parameters["minValue"]);
        Assert.Equal("9223372036854775807", issue.Parameters["maxValue"]);
    }
}

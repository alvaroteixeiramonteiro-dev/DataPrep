using DataPrep.Core.Validation;
using DataPrep.Core.Validation.Rules;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataPrep.Tests.Validation;

/// <summary>
/// Verifica os limites de comprimento, os dados do diagnóstico
/// e a rejeição de configurações inválidas.
/// </summary>
public sealed class MaxLengthRuleTests
{
    /// <summary>
    /// Valores ausentes ou dentro do limite não infringem esta regra.
    /// Inclui o caso de comprimento exatamente igual ao máximo.
    /// </summary>
    [Theory]
    [InlineData(null, 5)]
    [InlineData("", 0)]
    [InlineData("Ana", 5)]
    [InlineData("Ana", 3)]
    public void Validate_WhenWithinLimit_ReturnsNoIssue(string? value,int maxLength)
    {
        // Preparação.
        var rule = new MaxLengthRule();

        // Execução.
        var issue = rule.Validate(
            value: value,
            maxLength: maxLength,
            sourceField: "nome");

        // Verificação.
        Assert.Null(issue);
    }

    /// <summary>
    /// Um texto acima do limite deve produzir um diagnóstico completo,
    /// conservando os espaços e o restante conteúdo original.
    /// </summary>
    [Theory]
    [InlineData("Mesa", 3, "4")]
    [InlineData(" Ana ", 3, "5")]
    [InlineData("A", 0, "1")]
    public void Validate_WhenLimitExceeded_ReturnsLengthIssue(string value, int maxLength,
        string expectedLength)
    {
        // Preparação.
        var rule = new MaxLengthRule();

        // Execução: simula a posição do campo num ficheiro CSV.
        var issue = rule.Validate(
            value: value,
            maxLength: maxLength,
            sourceField: "nome",
            targetField: "name",
            recordNumber: 2,
            startLineNumber: 3,
            endLineNumber: 3,
            columnNumber: 1);

        // Verificação da regra e da orientação de correção.
        Assert.NotNull(issue);
        Assert.Equal("validation.max_length", issue.RuleCode);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("correction.shorten_value", issue.CorrectionCode);

        // Confirma que o diagnóstico permite localizar o problema
        // e consultar o valor sem qualquer corte ou transformação.
        Assert.Equal(value, issue.OriginalValue);
        Assert.Equal("nome", issue.SourceField);
        Assert.Equal("name", issue.TargetField);
        Assert.Equal(2L, issue.RecordNumber);
        Assert.Equal(3L, issue.StartLineNumber);
        Assert.Equal(3L, issue.EndLineNumber);
        Assert.Equal(1, issue.ColumnNumber);

        // Estes casos utilizam limites de um algarismo.
        Assert.Equal(
            ((char)('0' + maxLength)).ToString(),
            issue.Parameters["maxLength"]);

        Assert.Equal(expectedLength, issue.Parameters["actualLength"]);
    }

    /// <summary>
    /// Um limite negativo deve ser rejeitado como erro de configuração.
    /// </summary>
    [Fact]
    public void Validate_WhenLimitIsNegative_ThrowsException()
    {
        // Preparação.
        var rule = new MaxLengthRule();

        // Execução e verificação da configuração inválida.
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            rule.Validate(
                value: "Ana",
                maxLength: -1,
                sourceField: "nome"));
    }
}


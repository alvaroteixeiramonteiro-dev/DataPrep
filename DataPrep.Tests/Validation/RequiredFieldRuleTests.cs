using System;
using System.Collections.Generic;
using System.Text;
using DataPrep.Core.Validation;
using DataPrep.Core.Validation.Rules;
using Xunit;

namespace DataPrep.Tests.Validation;

/// <summary>
/// Verifica a deteção de campos obrigatórios vazios
/// e a preservação dos dados necessários ao diagnóstico.
/// </summary>
public sealed class RequiredFieldRuleTests
{
    /// <summary>
    /// Valores ausentes, vazios ou compostos apenas por espaços
    /// devem produzir um erro com a localização e o valor originais.
    /// </summary>
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    [InlineData("\r\n")]
    public void Validate_WhenValueIsBlank_ReturnsRequiredFieldIssue(
        string? value)
    {
        // Preparação: cria a regra que será testada.
        var rule = new RequiredFieldRule();

        // Execução: simula um campo localizado num registo CSV.
        var issue = rule.Validate(
            value: value,
            sourceField: "nome",
            targetField: "name",
            recordNumber: 12,
            startLineNumber: 13,
            endLineNumber: 13,
            columnNumber: 2);

        // Verificação: confirma a regra infringida e a correção indicada.
        Assert.NotNull(issue);
        Assert.Equal("validation.required", issue.RuleCode);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("correction.provide_value", issue.CorrectionCode);

        // A localização e o valor devem ser preservados,
        // incluindo a distinção entre null, vazio e espaços.
        Assert.Equal(12L, issue.RecordNumber);
        Assert.Equal(13L, issue.StartLineNumber);
        Assert.Equal(13L, issue.EndLineNumber);
        Assert.Equal(2, issue.ColumnNumber);
        Assert.Equal("nome", issue.SourceField);
        Assert.Equal("name", issue.TargetField);
        Assert.Equal(value, issue.OriginalValue);
    }

    /// <summary>
    /// Um valor preenchido cumpre esta regra, mesmo que outras regras
    /// possam posteriormente identificar problemas no seu conteúdo.
    /// </summary>
    [Theory]
    [InlineData("Ana")]
    [InlineData(" Ana ")]
    [InlineData("0")]
    public void Validate_WhenValueIsProvided_ReturnsNoIssue(string value)
    {
        // Preparação.
        var rule = new RequiredFieldRule();

        // Execução.
        var issue = rule.Validate(
            value: value,
            sourceField: "nome");

        // Verificação: esta regra não encontrou nenhum problema.
        Assert.Null(issue);
    }
}

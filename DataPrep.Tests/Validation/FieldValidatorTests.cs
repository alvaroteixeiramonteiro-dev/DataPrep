using DataPrep.Core.Validation;

namespace DataPrep.Tests.Validation;

/// <summary>
/// Verifica a combinação das regras e a transmissão da localização.
/// Os detalhes de cada regra são testados nas respetivas classes.
/// </summary>
public sealed class FieldValidatorTests
{
    /// <summary>
    /// Um campo que cumpre todas as regras não produz diagnósticos.
    /// </summary>
    [Fact]
    public void Validate_WhenAllRulesPass_ReturnsEmptyCollection()
    {
        // Preparação: inteiro preenchido e dentro do comprimento.
        var validator = new FieldValidator();
        var request = new FieldValidationRequest
        {
            Value = "123",
            SourceField = "quantidade",
            IsRequired = true,
            MaxLength = 3,
            MustBeInteger = true
        };

        // Execução.
        var issues = validator.Validate(request);

        // Verificação.
        Assert.Empty(issues);
    }

    /// <summary>
    /// Um campo obrigatório vazio produz o erro de obrigatoriedade,
    /// sem acrescentar um erro de formato inteiro.
    /// </summary>
    [Fact]
    public void Validate_WhenRequiredValueIsEmpty_ReturnsRequiredIssue()
    {
        // Preparação.
        var validator = new FieldValidator();
        var request = new FieldValidationRequest
        {
            Value = "",
            SourceField = "quantidade",
            IsRequired = true,
            MaxLength = 3,
            MustBeInteger = true
        };

        // Execução.
        var issues = validator.Validate(request);

        // Verificação: existe exatamente um problema.
        var issue = Assert.Single(issues);
        Assert.Equal("validation.required", issue.RuleCode);
    }

    /// <summary>
    /// Um campo opcional ausente é aceite mesmo com outras regras ativas.
    /// </summary>
    [Fact]
    public void Validate_WhenOptionalValueIsMissing_ReturnsEmptyCollection()
    {
        // Preparação.
        var validator = new FieldValidator();
        var request = new FieldValidationRequest
        {
            Value = null,
            SourceField = "quantidade",
            IsRequired = false,
            MaxLength = 3,
            MustBeInteger = true
        };

        // Execução e verificação.
        Assert.Empty(validator.Validate(request));
    }

    /// <summary>
    /// Reúne múltiplos problemas e conserva a localização em cada um.
    /// </summary>
    [Fact]
    public void Validate_WhenMultipleRulesFail_ReturnsAllIssues()
    {
        // Preparação: texto demasiado longo e que não representa um inteiro.
        var validator = new FieldValidator();
        var request = new FieldValidationRequest
        {
            Value = "abcde",
            SourceField = "quantidade",
            TargetField = "quantity",
            IsRequired = true,
            MaxLength = 3,
            MustBeInteger = true,
            RecordNumber = 4,
            StartLineNumber = 5,
            EndLineNumber = 5,
            ColumnNumber = 2
        };

        // Execução.
        var issues = validator.Validate(request);

        // Verificação: ambos os problemas devem ser apresentados.
        Assert.Equal(2, issues.Count);
        Assert.Contains(issues, i => i.RuleCode == "validation.max_length");
        Assert.Contains(issues, i => i.RuleCode == "validation.integer");

        // Todos os diagnósticos devem apontar para o mesmo campo original.
        Assert.All(issues, issue =>
        {
            Assert.Equal("abcde", issue.OriginalValue);
            Assert.Equal("quantidade", issue.SourceField);
            Assert.Equal("quantity", issue.TargetField);
            Assert.Equal(4L, issue.RecordNumber);
            Assert.Equal(5L, issue.StartLineNumber);
            Assert.Equal(5L, issue.EndLineNumber);
            Assert.Equal(2, issue.ColumnNumber);
        });
    }

    /// <summary>
    /// Regras desativadas não devem produzir problemas.
    /// </summary>
    [Fact]
    public void Validate_WhenRulesAreDisabled_ReturnsEmptyCollection()
    {
        // Preparação: sem regras ativas, o texto não será rejeitado.
        var validator = new FieldValidator();
        var request = new FieldValidationRequest
        {
            Value = "texto",
            SourceField = "observacoes"
        };

        // Execução e verificação.
        Assert.Empty(validator.Validate(request));
    }

    /// <summary>
    /// Uma configuração inválida é rejeitada mesmo sem valor para validar.
    /// </summary>
    [Fact]
    public void Validate_WhenLimitIsNegative_ThrowsException()
    {
        // Preparação.
        var validator = new FieldValidator();
        var request = new FieldValidationRequest
        {
            Value = null,
            SourceField = "nome",
            MaxLength = -1
        };

        // Execução e verificação.
        Assert.Throws<ArgumentOutOfRangeException>(
            () => validator.Validate(request));
    }
}

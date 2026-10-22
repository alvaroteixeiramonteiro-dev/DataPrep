using DataPrep.Core.Validation;
using DataPrep.Infrastructure.Csv;

namespace DataPrep.Tests.Csv;

/// <summary>
/// Verifica o percurso entre leitura, estrutura, mapeamento
/// e diagnósticos produzidos pelas regras dos campos.
/// </summary>
public sealed class CsvValidationServiceTests
{
    /// <summary>
    /// Um CSV com estrutura e valores corretos não produz problemas.
    /// </summary>
    [Fact]
    public void Validate_WhenDataIsValid_ReturnsNoIssues()
    {
        // Preparação: lê um CSV em memória.
        using var reader = new StringReader(
            "codigo;quantidade\nP1;10");

        var csv = new CsvDataReader().Read(reader);

        var mappings = new[]
        {
            new FieldMapping
            {
                SourceColumnNumber = 1,
                IsRequired = true,
                MaxLength = 10
            },
            new FieldMapping
            {
                SourceColumnNumber = 2,
                IsRequired = true,
                MustBeInteger = true
            }
        };

        // Execução.
        var issues = new CsvValidationService().Validate(
            csv, mappings, expectedColumnCount: 2);

        // Verificação.
        Assert.Empty(issues);
    }

    /// <summary>
    /// Um registo com colunas em falta ou a mais produz um erro estrutural.
    /// As regras de campos não são executadas nesse registo.
    /// </summary>
    [Theory]
    [InlineData("P1", "1")]
    [InlineData("P1;abc;extra", "3")]
    public void Validate_WhenColumnCountDiffers_ReturnsStructuralIssue(
        string row,
        string actualCount)
    {
        // Preparação.
        using var reader = new StringReader(
            $"codigo;quantidade\n{row}");

        var csv = new CsvDataReader().Read(reader);

        var mappings = new[]
        {
            new FieldMapping
            {
                SourceColumnNumber = 2,
                IsRequired = true,
                MustBeInteger = true
            }
        };

        // Execução.
        var issues = new CsvValidationService().Validate(
            csv, mappings, expectedColumnCount: 2);

        // Verificação: apenas o erro estrutural deve ser apresentado.
        var issue = Assert.Single(issues);

        Assert.Equal("validation.column_count", issue.RuleCode);
        Assert.Equal(ValidationSeverity.Error, issue.Severity);
        Assert.Equal("correction.fix_column_count", issue.CorrectionCode);
        Assert.Equal("2", issue.Parameters["expectedCount"]);
        Assert.Equal(actualCount, issue.Parameters["actualCount"]);
        Assert.Equal(1L, issue.RecordNumber);
        Assert.Equal(2L, issue.StartLineNumber);
        Assert.Equal(2L, issue.EndLineNumber);
        Assert.Null(issue.ColumnNumber);
    }

    /// <summary>
    /// O mapeamento por posição permite escolher campos fora da ordem
    /// de destino e conserva a localização após um registo multilinha.
    /// </summary>
    [Fact]
    public void Validate_WhenFieldIsInvalid_PreservesMappingAndLocation()
    {
        // Preparação: o primeiro registo ocupa duas linhas e não tem erros.
        using var reader = new StringReader(
            "descricao;quantidade;codigo\n" +
            "\"Mesa\nmadeira\";10;P1\n" +
            "Cadeira;abc;P2");

        var csv = new CsvDataReader().Read(reader);

        var mappings = new[]
        {
            new FieldMapping
            {
                SourceColumnNumber = 3,
                TargetField = "code",
                IsRequired = true
            },
            new FieldMapping
            {
                SourceColumnNumber = 2,
                TargetField = "quantity",
                MustBeInteger = true
            }
        };

        // Execução.
        var issues = new CsvValidationService().Validate(
            csv, mappings, expectedColumnCount: 3);

        // Verificação: o erro deve apontar para o segundo registo,
        // que começa na quarta linha física do ficheiro.
        var issue = Assert.Single(issues);

        Assert.Equal("validation.integer", issue.RuleCode);
        Assert.Equal("quantidade", issue.SourceField);
        Assert.Equal("quantity", issue.TargetField);
        Assert.Equal("abc", issue.OriginalValue);
        Assert.Equal(2, issue.ColumnNumber);
        Assert.Equal(2L, issue.RecordNumber);
        Assert.Equal(4L, issue.StartLineNumber);
        Assert.Equal(4L, issue.EndLineNumber);
    }

    /// <summary>
    /// Sem cabeçalho, o campo é identificado pela posição da coluna.
    /// </summary>
    [Fact]
    public void Validate_WithoutHeader_UsesColumnIdentifier()
    {
        // Preparação.
        using var reader = new StringReader("P1;abc");

        var csv = new CsvDataReader().Read(
            reader, hasHeader: false);

        var mappings = new[]
        {
            new FieldMapping
            {
                SourceColumnNumber = 2,
                MustBeInteger = true
            }
        };

        // Execução.
        var issues = new CsvValidationService().Validate(
            csv, mappings, expectedColumnCount: 2);

        // Verificação.
        var issue = Assert.Single(issues);
        Assert.Equal("column_2", issue.SourceField);
        Assert.Equal(1L, issue.StartLineNumber);
    }

    /// <summary>
    /// Um mapeamento fora da estrutura esperada é rejeitado,
    /// mesmo quando não existem registos para validar.
    /// </summary>
    [Fact]
    public void Validate_WhenMappingIsOutOfRange_ThrowsException()
    {
        // Preparação: CSV apenas com cabeçalho.
        using var reader = new StringReader("codigo;quantidade");
        var csv = new CsvDataReader().Read(reader);

        var mappings = new[]
        {
            new FieldMapping
            {
                SourceColumnNumber = 3
            }
        };

        // Execução e verificação.
        Assert.Throws<ArgumentException>(() =>
            new CsvValidationService().Validate(
                csv, mappings, expectedColumnCount: 2));
    }
}

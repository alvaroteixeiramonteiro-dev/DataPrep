using DataPrep.Infrastructure.Csv;

namespace DataPrep.Tests.Csv;

/// <summary>
/// Verifica a preservação dos valores e a localização dos registos CSV.
/// </summary>
public sealed class CsvDataReaderTests
{
    /// <summary>
    /// Lê o cabeçalho e conserva espaços e campos vazios.
    /// </summary>
    [Fact]
    public void Read_WithHeader_PreservesValuesAndLocation()
    {
        // Preparação: o espaço em " Mesa " não deve ser removido.
        using var reader = new StringReader(
            "codigo;nome;quantidade\r\nP1; Mesa ;");

        var service = new CsvDataReader();

        // Execução.
        var result = service.Read(reader);

        // Verificação do cabeçalho e do registo.
        Assert.Equal(
            new[] { "codigo", "nome", "quantidade" },
            result.Headers.ToArray());

        var record = Assert.Single(result.Records);

        Assert.Equal(new[] { "P1", " Mesa ", "" }, record.Values.ToArray());
        Assert.Equal(1L, record.RecordNumber);
        Assert.Equal(2L, record.StartLineNumber);
        Assert.Equal(2L, record.EndLineNumber);
    }

    /// <summary>
    /// Interpreta separadores, aspas escapadas e quebras dentro de campos.
    /// Confirma também a localização do registo seguinte.
    /// </summary>
    [Fact]
    public void Read_WithQuotedMultilineField_TracksPhysicalLines()
    {
        // Preparação: o primeiro registo ocupa as linhas físicas 2 e 3.
        using var reader = new StringReader(
            "codigo;descricao\n" +
            "P1;\"Mesa; modelo \"\"A\"\"\nmadeira\"\n" +
            "P2;Cadeira");

        var service = new CsvDataReader();

        // Execução.
        var result = service.Read(reader);

        // Verificação do conteúdo interpretado e das linhas físicas.
        Assert.Equal(2, result.Records.Count);

        var first = result.Records[0];
        Assert.Equal("Mesa; modelo \"A\"\nmadeira", first.Values[1]);
        Assert.Equal(1L, first.RecordNumber);
        Assert.Equal(2L, first.StartLineNumber);
        Assert.Equal(3L, first.EndLineNumber);

        var second = result.Records[1];
        Assert.Equal(2L, second.RecordNumber);
        Assert.Equal(4L, second.StartLineNumber);
        Assert.Equal(4L, second.EndLineNumber);
    }

    /// <summary>
    /// Sem cabeçalho, o primeiro registo também é considerado dado.
    /// Verifica ainda a utilização de um separador diferente.
    /// </summary>
    [Fact]
    public void Read_WithoutHeader_KeepsFirstRecord()
    {
        // Preparação.
        using var reader = new StringReader("P1,Mesa\nP2,Cadeira");
        var service = new CsvDataReader();

        // Execução.
        var result = service.Read(
            reader,
            delimiter: ",",
            hasHeader: false);

        // Verificação.
        Assert.Empty(result.Headers);
        Assert.Equal(2, result.Records.Count);
        Assert.Equal("P1", result.Records[0].Values[0]);
        Assert.Equal(1L, result.Records[0].StartLineNumber);
    }

    /// <summary>
    /// Um ficheiro vazio não contém cabeçalhos nem registos.
    /// A decisão de o rejeitar pertence à validação do ficheiro.
    /// </summary>
    [Fact]
    public void Read_WhenEmpty_ReturnsEmptyCollections()
    {
        // Preparação.
        using var reader = new StringReader("");
        var service = new CsvDataReader();

        // Execução.
        var result = service.Read(reader);

        // Verificação.
        Assert.Empty(result.Headers);
        Assert.Empty(result.Records);
    }

    /// <summary>
    /// Linhas vazias não devem desaparecer nem deslocar a localização.
    /// </summary>
    [Fact]
    public void Read_WithBlankLine_PreservesRecordAndLineNumbers()
    {
        // Preparação: existe uma linha vazia entre os dois produtos.
        using var reader = new StringReader(
            "codigo;nome\nP1;Mesa\n\nP2;Cadeira");

        var service = new CsvDataReader();

        // Execução.
        var result = service.Read(reader);

        // Verificação: a linha vazia também ocupa um registo.
        Assert.Equal(3, result.Records.Count);
        Assert.Equal(3L, result.Records[1].StartLineNumber);
        Assert.Equal(3L, result.Records[1].EndLineNumber);
        Assert.Equal(4L, result.Records[2].StartLineNumber);
        Assert.Equal(3L, result.Records[2].RecordNumber);
    }
}
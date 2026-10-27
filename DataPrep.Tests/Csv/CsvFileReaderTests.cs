using System.Text;
using DataPrep.Infrastructure.Csv;

namespace DataPrep.Tests.Csv;

/// <summary>
/// Verifica a leitura de ficheiros reais e o tratamento da codificação.
/// Cada teste remove o ficheiro temporário que criou.
/// </summary>
public sealed class CsvFileReaderTests
{
    /// <summary>
    /// UTF-8 com e sem BOM deve preservar acentos e cabeçalhos.
    /// </summary>
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReadFile_WithUtf8_PreservesText(bool includeBom)
    {
        // Preparação: cria um ficheiro temporário exclusivo deste teste.
        string path = Path.GetTempFileName();

        try
        {
            var encoding = new UTF8Encoding(
                encoderShouldEmitUTF8Identifier: includeBom,
                throwOnInvalidBytes: true);

            File.WriteAllText(
                path,
                "codigo;nome\nP1;Maçã",
                encoding);

            // Execução.
            var result = new CsvDataReader().ReadFile(path);

            // Verificação: o BOM não deve fazer parte do cabeçalho.
            Assert.True(result.IsComplete);
            Assert.Empty(result.ReadIssues);
            Assert.Equal("codigo", result.Headers[0]);

            var record = Assert.Single(result.Records);
            Assert.Equal("Maçã", record.Values[1]);
            Assert.Equal(2L, record.StartLineNumber);
        }
        finally
        {
            // A limpeza também ocorre quando uma asserção falha.
            File.Delete(path);
        }
    }

    /// <summary>
    /// Bytes inválidos para UTF-8 produzem um diagnóstico de codificação,
    /// sem devolver conteúdo parcial como se estivesse pronto a validar.
    /// </summary>
    [Fact]
    public void ReadFile_WithInvalidUtf8_ReturnsEncodingIssue()
    {
        // Preparação.
        string path = Path.GetTempFileName();

        try
        {
            // 0xFF não é um byte válido numa sequência UTF-8.
            File.WriteAllBytes(
                path,
                new byte[] { 0x63, 0x0A, 0xFF });

            // Execução.
            var result = new CsvDataReader().ReadFile(path);

            // Verificação.
            Assert.False(result.IsComplete);
            Assert.Empty(result.Headers);
            Assert.Empty(result.Records);

            var issue = Assert.Single(result.ReadIssues);

            Assert.Equal("validation.csv_encoding", issue.RuleCode);
            Assert.Equal("correction.save_as_utf8", issue.CorrectionCode);
            Assert.Equal("UTF-8", issue.Parameters["encoding"]);
            Assert.Null(issue.StartLineNumber);
            Assert.Null(issue.ColumnNumber);
        }
        finally
        {
            File.Delete(path);
        }
    }

    /// <summary>
    /// Um ficheiro inexistente é uma falha operacional,
    /// não um erro de sintaxe ou codificação dos dados.
    /// </summary>
    [Fact]
    public void ReadFile_WhenFileDoesNotExist_ThrowsFileNotFoundException()
    {
        // Preparação: gera um caminho sem criar o ficheiro.
        string path = Path.Combine(
            Path.GetTempPath(),
            $"dataprep-{Guid.NewGuid():N}.csv");

        // Execução e verificação.
        Assert.Throws<FileNotFoundException>(
            () => new CsvDataReader().ReadFile(path));
    }
}

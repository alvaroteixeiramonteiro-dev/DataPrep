namespace DataPrep.Infrastructure.Csv;

/// <summary>
/// Representa um registo CSV com os valores lidos e a sua localização.
/// Os valores ainda não foram convertidos nem transformados.
/// </summary>
public sealed class CsvDataRecord
{
    /// <summary>
    /// Número do registo de dados, começando em 1 e excluindo o cabeçalho.
    /// </summary>
    public required long RecordNumber { get; init; }

    /// <summary>
    /// Primeira linha física ocupada pelo registo.
    /// </summary>
    public required long StartLineNumber { get; init; }

    /// <summary>
    /// Última linha física ocupada pelo registo.
    /// Pode diferir da primeira se existirem campos com quebras de linha.
    /// </summary>
    public required long EndLineNumber { get; init; }

    /// <summary>
    /// Valores pela ordem das colunas.
    /// As aspas e os escapes do formato CSV já foram interpretados.
    /// Os espaços e o conteúdo dos campos são preservados.
    /// </summary>
    public required IReadOnlyList<string> Values { get; init; }
}

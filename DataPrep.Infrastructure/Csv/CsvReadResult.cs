namespace DataPrep.Infrastructure.Csv;

/// <summary>
/// Contém o resultado da leitura de um CSV.
/// Nesta primeira versão, todos os registos ficam em memória.
/// </summary>
public sealed class CsvReadResult
{
    /// <summary>
    /// Cabeçalhos na ordem original.
    /// A coleção está vazia quando o ficheiro não tem cabeçalho.
    /// Nomes vazios ou repetidos são preservados para validação posterior.
    /// </summary>
    public required IReadOnlyList<string> Headers { get; init; }

    /// <summary>
    /// Registos de dados, excluindo o cabeçalho.
    /// </summary>
    public required IReadOnlyList<CsvDataRecord> Records { get; init; }
}

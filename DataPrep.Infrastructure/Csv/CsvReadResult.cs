using DataPrep.Core.Validation;

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

    /// <summary>
    /// Última linha física ocupada pelo cabeçalho.
    /// É null quando não foi lido um cabeçalho.
    /// O cabeçalho começa na primeira linha física do ficheiro.
    /// </summary>
    public long? HeaderEndLineNumber { get; init; }

    /// <summary>
    /// Problemas de sintaxe encontrados durante a leitura.
    /// Uma leitura interrompida pode conter apenas os registos anteriores ao erro.
    /// </summary>
    public IReadOnlyList<ValidationIssue> ReadIssues { get; init; }
        = Array.Empty<ValidationIssue>();

    /// <summary>
    /// Indica se a leitura terminou sem erros bloqueantes.
    /// Não significa que os dados já passaram nas regras de validação.
    /// </summary>
    public bool IsComplete =>
        !ReadIssues.Any(issue => issue.Severity == ValidationSeverity.Error);
}

namespace DataPrep.Core.Validation;

/// <summary>
/// Associa uma coluna de origem a um campo de destino
/// e define as regras aplicáveis aos seus valores.
/// </summary>
public sealed class FieldMapping
{
    /// <summary>
    /// Posição da coluna no CSV, começando em 1.
    /// A posição permite identificar colunas mesmo com nomes repetidos.
    /// </summary>
    public required int SourceColumnNumber { get; init; }

    /// <summary>
    /// Nome do campo de destino.
    /// Pode ser null enquanto não existir um destino SQL definido.
    /// </summary>
    public string? TargetField { get; init; }

    /// <summary>
    /// Indica se o campo tem de estar preenchido.
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Comprimento máximo; null desativa esta regra.
    /// </summary>
    public int? MaxLength { get; init; }

    /// <summary>
    /// Indica se o valor deve representar um número inteiro.
    /// </summary>
    public bool MustBeInteger { get; init; }
}

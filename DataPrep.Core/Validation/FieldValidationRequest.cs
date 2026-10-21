using System;
using System.Collections.Generic;
using System.Text;

namespace DataPrep.Core.Validation;

/// <summary>
/// Reúne o valor, a localização e as regras aplicáveis a um campo.
/// Não contém mensagens traduzidas nem depende da base de dados.
/// </summary>
public sealed class FieldValidationRequest
{
    /// <summary>
    /// Valor original do campo, antes de qualquer transformação.
    /// </summary>
    public string? Value { get; init; }

    /// <summary>
    /// Nome do campo no ficheiro de origem.
    /// </summary>
    public required string SourceField { get; init; }

    /// <summary>
    /// Nome do campo de destino, quando existir um mapeamento.
    /// </summary>
    public string? TargetField { get; init; }

    /// <summary>
    /// Indica se o campo tem de estar preenchido.
    /// </summary>
    public bool IsRequired { get; init; }

    /// <summary>
    /// Comprimento máximo permitido.
    /// null significa que esta regra não será aplicada.
    /// </summary>
    public int? MaxLength { get; init; }

    /// <summary>
    /// Indica se o conteúdo deve representar um número inteiro.
    /// </summary>
    public bool MustBeInteger { get; init; }

    /// <summary>
    /// Número do registo, começando em 1 e excluindo o cabeçalho.
    /// </summary>
    public long? RecordNumber { get; init; }

    /// <summary>
    /// Primeira linha física do registo, incluindo o cabeçalho na contagem.
    /// </summary>
    public long? StartLineNumber { get; init; }

    /// <summary>
    /// Última linha física ocupada pelo registo.
    /// </summary>
    public long? EndLineNumber { get; init; }

    /// <summary>
    /// Posição da coluna de origem, começando em 1.
    /// </summary>
    public int? ColumnNumber { get; init; }
}

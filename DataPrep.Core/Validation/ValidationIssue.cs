using System;
using System.Collections.Generic;
using System.Text;

namespace DataPrep.Core.Validation;

/// <summary>
/// Representa um problema encontrado durante a validação dos dados.
/// Guarda códigos e dados estruturados, sem mensagens traduzidas,
/// para permitir a apresentação do diagnóstico em diferentes idiomas.
/// </summary>
public sealed class ValidationIssue
{
    /// <summary>
    /// Código estável da regra infringida.
    /// Exemplo: "validation.required".
    /// A interface utiliza este código para obter a mensagem traduzida.
    /// </summary>
    public required string RuleCode { get; init; }
    /// <summary>
    /// Gravidade do problema, que determina se bloqueia a importação.
    /// </summary>
    public required ValidationSeverity Severity { get; init; }
    /// <summary>
    /// Número do registo de dados, começando em 1 e excluindo o cabeçalho.
    /// É null quando o problema não está associado a um registo específico.
    /// </summary>
    public long? RecordNumber { get; init; }
    /// <summary>
    /// Primeira linha física ocupada pelo registo no ficheiro.
    /// A contagem começa em 1 e inclui o cabeçalho.
    /// É null quando a localização não está disponível ou não se aplica.
    /// </summary>
    public long? StartLineNumber { get; init; }
    /// <summary>
    /// Última linha física ocupada pelo registo no ficheiro.
    /// Pode diferir da primeira porque um campo CSV entre aspas
    /// pode conter quebras de linha.
    /// É null quando a localização não está disponível ou não se aplica.
    /// </summary>
    public long? EndLineNumber { get; init; }
    /// <summary>
    /// Posição da coluna de origem no CSV, começando em 1.
    /// É null quando o problema não está associado a uma coluna específica.
    /// </summary>
    public int? ColumnNumber { get; init; }
    /// <summary>
    /// Nome do campo de origem no CSV.
    /// É null quando o nome não está disponível ou não se aplica.
    /// </summary>
    public string? SourceField { get; init; }
    /// <summary>
    /// Nome do campo de destino na base de dados.
    /// É null quando ainda não existe um mapeamento ou não se aplica.
    /// </summary>
    public string? TargetField { get; init; }
    /// <summary>
    /// Valor do campo tal como foi lido, antes das transformações.
    /// Não corresponde necessariamente ao texto literal do CSV:
    /// as aspas e os escapes do formato são tratados pelo leitor.
    /// null indica um campo indisponível; "" indica um campo presente e vazio.
    /// </summary>
    public string? OriginalValue { get; init; }
    /// <summary>
    /// Código estável da instrução de correção.
    /// Exemplo: "correction.provide_value".
    /// A interface utiliza este código para obter a orientação traduzida.
    /// </summary>
    public required string CorrectionCode { get; init; }
    /// <summary>
    /// Parâmetros utilizados para completar as mensagens traduzidas.
    /// Exemplo: "maxLength" com o valor "100".
    /// Os valores devem usar uma representação estável e independente
    /// do idioma da interface.
    /// </summary>
    public IReadOnlyDictionary<string, string> Parameters { get; init; }
        = new Dictionary<string, string>();
}

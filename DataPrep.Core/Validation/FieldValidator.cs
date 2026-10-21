using DataPrep.Core.Validation.Rules;

namespace DataPrep.Core.Validation;

/// <summary>
/// Executa as regras configuradas para um campo
/// e reúne os diagnósticos encontrados.
/// </summary>
public sealed class FieldValidator
{
    // As regras não guardam dados entre chamadas,
    // pelo que podem ser reutilizadas pelo validador.
    private readonly RequiredFieldRule _requiredRule = new();
    private readonly MaxLengthRule _maxLengthRule = new();
    private readonly IntegerRule _integerRule = new();

    /// <summary>
    /// Devolve os problemas encontrados.
    /// Uma coleção vazia significa que o campo passou nas regras aplicadas.
    /// </summary>
    /// <param name="request">
    /// Valor, localização e configuração das regras do campo.
    /// </param>
    public IReadOnlyList<ValidationIssue> Validate(
        FieldValidationRequest request)
    {
        // Uma chamada sem pedido representa um erro de programação.
        ArgumentNullException.ThrowIfNull(request);

        // Rejeita configurações inválidas antes de avaliar os dados.
        if (request.MaxLength is int configuredLimit)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(configuredLimit);
        }

        var issues = new List<ValidationIssue>();

        // A obrigatoriedade só é verificada quando estiver configurada.
        if (request.IsRequired)
        {
            AddIfPresent(_requiredRule.Validate(
                value: request.Value,
                sourceField: request.SourceField,
                targetField: request.TargetField,
                recordNumber: request.RecordNumber,
                startLineNumber: request.StartLineNumber,
                endLineNumber: request.EndLineNumber,
                columnNumber: request.ColumnNumber));
        }

        // Um limite null desativa a regra de comprimento.
        if (request.MaxLength is int maxLength)
        {
            AddIfPresent(_maxLengthRule.Validate(
                value: request.Value,
                maxLength: maxLength,
                sourceField: request.SourceField,
                targetField: request.TargetField,
                recordNumber: request.RecordNumber,
                startLineNumber: request.StartLineNumber,
                endLineNumber: request.EndLineNumber,
                columnNumber: request.ColumnNumber));
        }

        // A verificação do formato inteiro é independente do comprimento.
        if (request.MustBeInteger)
        {
            AddIfPresent(_integerRule.Validate(
                value: request.Value,
                sourceField: request.SourceField,
                targetField: request.TargetField,
                recordNumber: request.RecordNumber,
                startLineNumber: request.StartLineNumber,
                endLineNumber: request.EndLineNumber,
                columnNumber: request.ColumnNumber));
        }

        // Expõe os resultados através de uma coleção apenas de leitura.
        return issues.AsReadOnly();

        // Função local: acrescenta apenas diagnósticos existentes.
        // Um resultado null de uma regra não representa um problema.
        void AddIfPresent(ValidationIssue? issue)
        {
            if (issue is not null)
            {
                issues.Add(issue);
            }
        }
    }
}

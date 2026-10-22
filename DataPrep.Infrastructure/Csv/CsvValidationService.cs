using System.Globalization;
using DataPrep.Core.Validation;

namespace DataPrep.Infrastructure.Csv;

/// <summary>
/// Verifica a estrutura dos registos CSV e encaminha
/// os campos mapeados para o validador de campos.
/// </summary>
public sealed class CsvValidationService
{
    // O validador não mantém dados específicos de cada campo.
    private readonly FieldValidator _fieldValidator = new();

    /// <summary>
    /// Valida a quantidade de colunas e os campos mapeados.
    /// Devolve todos os diagnósticos encontrados.
    /// </summary>
    /// <param name="csv">Resultado da leitura do CSV.</param>
    /// <param name="mappings">Colunas e regras que serão aplicadas.</param>
    /// <param name="expectedColumnCount">
    /// Número esperado de colunas.
    /// Com cabeçalho, deve corresponder ao número de cabeçalhos lidos.
    /// Sem cabeçalho, deve ser definido explicitamente pelo utilizador.
    /// </param>
    public IReadOnlyList<ValidationIssue> Validate(
        CsvReadResult csv,
        IReadOnlyList<FieldMapping> mappings,
        int expectedColumnCount)
    {
        // Estes casos representam uma configuração inválida do serviço.
        ArgumentNullException.ThrowIfNull(csv);
        ArgumentNullException.ThrowIfNull(mappings);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(
            expectedColumnCount);

        // Uma leitura incompleta não deve produzir diagnósticos adicionais
        // sobre cabeçalhos ou registos que podem estar apenas parcialmente lidos.
        if (!csv.IsComplete)
        {
            return csv.ReadIssues;
        }

        // Havendo cabeçalho, a estrutura esperada é definida por ele.
        if (csv.Headers.Count > 0 &&
            csv.Headers.Count != expectedColumnCount)
        {
            throw new ArgumentException(
                "Expected column count must match the CSV header count.",
                nameof(expectedColumnCount));
        }

        // Valida toda a configuração antes de começar a analisar os dados,
        // mesmo que o ficheiro não contenha registos.
        var mappedColumns = new HashSet<int>();

        foreach (var mapping in mappings)
        {
            ArgumentNullException.ThrowIfNull(mapping);

            if (mapping.SourceColumnNumber < 1 ||
                mapping.SourceColumnNumber > expectedColumnCount)
            {
                throw new ArgumentException(
                    "A mapped column is outside the expected column range.",
                    nameof(mappings));
            }

            // Nesta versão, cada coluna de origem só pode ser mapeada uma vez.
            if (!mappedColumns.Add(mapping.SourceColumnNumber))
            {
                throw new ArgumentException(
                    "A source column cannot be mapped more than once.",
                    nameof(mappings));
            }

            if (mapping.MaxLength is int limit)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(limit);
            }
        }

        var issues = new List<ValidationIssue>();

        // Sem cabeçalho nem registos, não existe conteúdo para importar.
        if (csv.Headers.Count == 0 && csv.Records.Count == 0)
        {
            issues.Add(new ValidationIssue
            {
                RuleCode = "validation.empty_file",
                Severity = ValidationSeverity.Error,
                CorrectionCode = "correction.provide_csv_content"
            });

            return issues.AsReadOnly();
        }

        // A comparação é exata e independente do idioma.
        // Nesta versão, "nome" e "Nome" são cabeçalhos diferentes.
        var firstColumnByHeader = new Dictionary<string, int>(
            StringComparer.Ordinal);

        for (int index = 0; index < csv.Headers.Count; index++)
        {
            string header = csv.Headers[index];
            int columnNumber = index + 1;

            if (string.IsNullOrWhiteSpace(header))
            {
                // Conserva o cabeçalho original, incluindo espaços.
                issues.Add(new ValidationIssue
                {
                    RuleCode = "validation.header_required",
                    Severity = ValidationSeverity.Error,
                    StartLineNumber = 1,
                    EndLineNumber = csv.HeaderEndLineNumber,
                    ColumnNumber = columnNumber,
                    SourceField = header,
                    OriginalValue = header,
                    CorrectionCode = "correction.provide_header"
                });

                continue;
            }

            if (firstColumnByHeader.TryGetValue(
                header, out int firstColumnNumber))
            {
                // Identifica a coluna repetida e indica onde surgiu primeiro.
                issues.Add(new ValidationIssue
                {
                    RuleCode = "validation.duplicate_header",
                    Severity = ValidationSeverity.Error,
                    StartLineNumber = 1,
                    EndLineNumber = csv.HeaderEndLineNumber,
                    ColumnNumber = columnNumber,
                    SourceField = header,
                    OriginalValue = header,
                    CorrectionCode = "correction.rename_duplicate_header",
                    Parameters = new Dictionary<string, string>
                    {
                        ["firstColumnNumber"] = firstColumnNumber.ToString(
                            CultureInfo.InvariantCulture)
                    }
                });
            }
            else
            {
                firstColumnByHeader.Add(header, columnNumber);
            }
        }

        // Os cabeçalhos devem ser corrigidos antes da validação dos dados,
        // para que a identificação dos campos seja clara.
        if (issues.Count > 0)
        {
            return issues.AsReadOnly();
        }

        // Um ficheiro apenas com cabeçalho também não tem dados para importar.
        if (csv.Records.Count == 0)
        {
            issues.Add(new ValidationIssue
            {
                RuleCode = "validation.no_data_records",
                Severity = ValidationSeverity.Error,
                CorrectionCode = "correction.provide_data_records"
            });

            return issues.AsReadOnly();
        }

        foreach (var record in csv.Records)
        {
            if (record.Values.Count != expectedColumnCount)
            {
                // O problema afeta a estrutura do registo.
                // Não inventamos um campo ou valor específico para este erro.
                issues.Add(new ValidationIssue
                {
                    RuleCode = "validation.column_count",
                    Severity = ValidationSeverity.Error,
                    RecordNumber = record.RecordNumber,
                    StartLineNumber = record.StartLineNumber,
                    EndLineNumber = record.EndLineNumber,
                    CorrectionCode = "correction.fix_column_count",
                    Parameters = new Dictionary<string, string>
                    {
                        ["expectedCount"] = expectedColumnCount.ToString(
                            CultureInfo.InvariantCulture),

                        ["actualCount"] = record.Values.Count.ToString(
                            CultureInfo.InvariantCulture)
                    }
                });

                // Com uma estrutura incorreta, não podemos garantir
                // que os valores estejam alinhados com os campos previstos.
                continue;
            }

            foreach (var mapping in mappings)
            {
                // As posições públicas começam em 1; os índices começam em 0.
                int index = mapping.SourceColumnNumber - 1;

                // Sem cabeçalho, utiliza um identificador estável por posição.
                // A interface poderá apresentar uma designação traduzida.
                string sourceField = csv.Headers.Count > 0
                    ? csv.Headers[index]
                    : $"column_{mapping.SourceColumnNumber.ToString(
                        CultureInfo.InvariantCulture)}";

                var request = new FieldValidationRequest
                {
                    Value = record.Values[index],
                    SourceField = sourceField,
                    TargetField = mapping.TargetField,
                    IsRequired = mapping.IsRequired,
                    MaxLength = mapping.MaxLength,
                    MustBeInteger = mapping.MustBeInteger,
                    RecordNumber = record.RecordNumber,
                    StartLineNumber = record.StartLineNumber,
                    EndLineNumber = record.EndLineNumber,
                    ColumnNumber = mapping.SourceColumnNumber
                };

                // Reúne os problemas de todas as regras aplicáveis ao campo.
                issues.AddRange(_fieldValidator.Validate(request));
            }
        }

        return issues.AsReadOnly();
    }
}

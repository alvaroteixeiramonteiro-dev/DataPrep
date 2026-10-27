using System.Text;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using DataPrep.Core.Validation;

namespace DataPrep.Infrastructure.Csv;

/// <summary>
/// Lê valores e localizações do CSV.
/// Converte erros de sintaxe em diagnósticos independentes do idioma.
/// </summary>
public sealed class CsvDataReader
{
    /// <summary>
    /// Abre e lê um ficheiro CSV codificado em UTF-8, com ou sem BOM.
    /// Fecha o ficheiro depois da leitura, mesmo quando ocorre um erro.
    /// </summary>
    /// <param name="path">Caminho do ficheiro.</param>
    /// <param name="delimiter">Separador escolhido para os campos.</param>
    /// <param name="hasHeader">Indica se existe cabeçalho.</param>
    public CsvReadResult ReadFile(
        string path,
        string delimiter = ";",
        bool hasHeader = true)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        // Lança uma exceção se encontrar bytes inválidos para UTF-8,
        // em vez de os substituir por caracteres de substituição.
        // O preâmbulo permite ao StreamReader reconhecer um BOM UTF-8.
        var encoding = new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: true,
            throwOnInvalidBytes: true);

        try
        {
            // Não deteta automaticamente outras codificações através do BOM.
            // Nesta etapa, o contrato do método é exclusivamente UTF-8.
            using var reader = new StreamReader(
                path,
                encoding,
                detectEncodingFromByteOrderMarks: false);

            return Read(reader, delimiter, hasHeader);
        }
        catch (DecoderFallbackException)
        {
            // Descarta qualquer leitura parcial.
            // Sem texto descodificado corretamente, não atribuímos uma linha
            // ou um campo que não conseguimos identificar com segurança.
            var issue = new ValidationIssue
            {
                RuleCode = "validation.csv_encoding",
                Severity = ValidationSeverity.Error,
                CorrectionCode = "correction.save_as_utf8",
                Parameters = new Dictionary<string, string>
                {
                    ["encoding"] = "UTF-8"
                }
            };

            return new CsvReadResult
            {
                Headers = Array.Empty<string>(),
                Records = Array.Empty<CsvDataRecord>(),
                ReadIssues = Array.AsReadOnly(new[] { issue })
            };
        }
    }
    /// <summary>
    /// Lê a partir da posição atual do leitor.
    /// O chamador continua responsável por fechar o leitor.
    /// A leitura termina no primeiro erro de sintaxe.
    /// </summary>
    /// <param name="reader">Origem do texto já descodificado.</param>
    /// <param name="delimiter">Separador escolhido para os campos.</param>
    /// <param name="hasHeader">Indica se existe cabeçalho.</param>
    public CsvReadResult Read(
        TextReader reader,
        string delimiter = ";",
        bool hasHeader = true)
    {
        // Configurações inválidas continuam a ser erros de utilização.
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentException.ThrowIfNullOrWhiteSpace(delimiter);

        var configuration = new CsvConfiguration(
            CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter,
            HasHeaderRecord = false,
            IgnoreBlankLines = false,
            TrimOptions = TrimOptions.None,
            ExceptionMessagesContainRawData = false,

            // Usa as regras de aspas e escapes do formato CSV.
            Mode = CsvMode.RFC4180
        };

        using var parser = new CsvParser(
            reader,
            configuration,
            leaveOpen: true);

        var headers = Array.Empty<string>();
        long? headerEndLineNumber = null;

        var records = new List<CsvDataRecord>();
        var readIssues = new List<ValidationIssue>();

        long previousEndLine = 0;
        long recordNumber = 0;
        bool isFirstRecord = true;

        while (true)
        {
            long startLine = previousEndLine + 1;
            bool readingHeader = isFirstRecord && hasHeader;

            // A posição só fica conhecida quando começamos a interpretar
            // individualmente os campos do registo.
            int? currentColumnNumber = null;

            try
            {
                if (!parser.Read())
                {
                    break;
                }

                long endLine = parser.RawRow;

                // Lê cada campo individualmente para identificar a coluna
                // caso o parser encontre sintaxe inválida nesse campo.
                var values = new string[parser.Count];

                for (int index = 0; index < parser.Count; index++)
                {
                    currentColumnNumber = index + 1;
                    values[index] = parser[index];
                }

                if (readingHeader)
                {
                    headers = values;
                    headerEndLineNumber = endLine;
                }
                else
                {
                    recordNumber++;

                    records.Add(new CsvDataRecord
                    {
                        RecordNumber = recordNumber,
                        StartLineNumber = startLine,
                        EndLineNumber = endLine,
                        Values = Array.AsReadOnly(values)
                    });
                }

                // Só avança o estado depois de interpretar o registo inteiro.
                previousEndLine = endLine;
                isFirstRecord = false;
            }
            catch (BadDataException exception)
            {
                // Um cabeçalho malformado ainda não tem nome fiável.
                // Nos dados, usa o nome apenas se a coluna estiver disponível.
                string? sourceField = null;

                if (!readingHeader &&
                    currentColumnNumber is int column &&
                    column <= headers.Length)
                {
                    sourceField = headers[column - 1];
                }

                readIssues.Add(new ValidationIssue
                {
                    RuleCode = "validation.csv_syntax",
                    Severity = ValidationSeverity.Error,
                    RecordNumber = readingHeader
                        ? null
                        : recordNumber + 1,

                    StartLineNumber = startLine,
                    EndLineNumber = Math.Max(startLine, parser.RawRow),
                    ColumnNumber = currentColumnNumber,
                    SourceField = sourceField,

                    // Não existe um valor interpretado com segurança.
                    // O fragmento bruto é guardado num parâmetro próprio.
                    OriginalValue = null,
                    CorrectionCode = "correction.fix_csv_quoting",
                    Parameters = new Dictionary<string, string>
                    {
                        ["rawField"] = exception.Field ?? "",
                        ["section"] = readingHeader ? "header" : "data"
                    }
                });

                // Não tenta recuperar o alinhamento depois de sintaxe inválida.
                break;
            }
        }

        return new CsvReadResult
        {
            Headers = Array.AsReadOnly(headers),
            HeaderEndLineNumber = headerEndLineNumber,
            Records = records.AsReadOnly(),
            ReadIssues = readIssues.AsReadOnly()
        };
    }
}

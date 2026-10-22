using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace DataPrep.Infrastructure.Csv;

/// <summary>
/// Lê a estrutura CSV sem converter os valores para tipos de negócio.
/// </summary>
public sealed class CsvDataReader
{
    /// <summary>
    /// Lê os dados a partir da posição atual do leitor.
    /// O código que fornece o leitor continua responsável por o fechar.
    /// </summary>
    /// <param name="reader">Origem do texto já descodificado.</param>
    /// <param name="delimiter">Separador escolhido, por exemplo ";" ou ",".</param>
    /// <param name="hasHeader">Indica se o primeiro registo é o cabeçalho.</param>
    public CsvReadResult Read(
        TextReader reader,
        string delimiter = ";",
        bool hasHeader = true)
    {
        // Argumentos inválidos representam erros de utilização do serviço.
        ArgumentNullException.ThrowIfNull(reader);
        ArgumentException.ThrowIfNullOrWhiteSpace(delimiter);

        var configuration = new CsvConfiguration(
            CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter,

            // O cabeçalho será tratado explicitamente pelo nosso código.
            HasHeaderRecord = false,

            // Não saltamos linhas vazias: também precisam de localização
            // e poderão produzir diagnósticos numa etapa posterior.
            IgnoreBlankLines = false,

            // Não removemos espaços durante a leitura.
            TrimOptions = TrimOptions.None,

            // Evita incluir o conteúdo original nas mensagens de exceção.
            ExceptionMessagesContainRawData = false
        };

        // O parser é fechado no fim, mas o leitor recebido fica aberto.
        using var parser = new CsvParser(
            reader,
            configuration,
            leaveOpen: true);

        var headers = Array.Empty<string>();
        // Guarda a localização do cabeçalho, incluindo cabeçalhos multilinha.
        long? headerEndLineNumber = null;
        var records = new List<CsvDataRecord>();

        long previousEndLine = 0;
        long recordNumber = 0;
        bool isFirstRecord = true;

        while (parser.Read())
        {
            // RawRow conta linhas físicas, incluindo quebras dentro de campos.
            long startLine = previousEndLine + 1;
            long endLine = parser.RawRow;
            previousEndLine = endLine;

            // Criamos uma cópia porque o parser mantém estado entre leituras.
            // O acesso a Record também força a interpretação dos campos.
            var values = parser.Record!.ToArray();

            if (isFirstRecord && hasHeader)
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

            isFirstRecord = false;
        }

        return new CsvReadResult
        {
            Headers = Array.AsReadOnly(headers),
            HeaderEndLineNumber = headerEndLineNumber,
            Records = records.AsReadOnly()
        };
    }
}

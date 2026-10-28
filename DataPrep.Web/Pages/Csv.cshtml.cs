using System.Text;
using DataPrep.Infrastructure.Csv;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DataPrep.Core.Validation;

namespace DataPrep.Web.Pages;

/// <summary>
/// Recebe um CSV e prepara os resultados para apresentação.
/// O ficheiro é lido diretamente, sem guardar uma cópia no servidor.
/// </summary>
// Limite total do pedido, incluindo os campos do formulário.
[RequestSizeLimit(6 * 1024 * 1024)]
public sealed class CsvModel : PageModel
{
    private const long MaxFileSize = 5 * 1024 * 1024;
    private readonly CsvDataReader _reader;
    private readonly CsvValidationService _validator;

    /// <summary>
    /// Recebe os serviços de leitura e validação registados na aplicação.
    /// </summary>
    public CsvModel(
        CsvDataReader reader,
        CsvValidationService validator)
    {
        _reader = reader;
        _validator = validator;
    }

    /// <summary>Ficheiro enviado pelo formulário.</summary>
    [BindProperty]
    public IFormFile? Upload { get; set; }

    /// <summary>Separador escolhido explicitamente.</summary>
    [BindProperty]
    public string Delimiter { get; set; } = ";";

    /// <summary>Indica se o primeiro registo é um cabeçalho.</summary>
    [BindProperty]
    public bool HasHeader { get; set; } = true;

    /// Quantidade de colunas definida pelo utilizador para CSV sem cabeçalho.
    /// Com cabeçalho, a quantidade é obtida a partir dos nomes lidos.
    /// </summary>
    [BindProperty]
    public int? ExpectedColumnCount { get; set; }

    /// <summary>Resultado da leitura, incluindo possíveis problemas.</summary>
    public CsvReadResult? Result { get; private set; }

    /// <summary>Código de uma mensagem da página a traduzir.</summary>
    public string? ErrorKey { get; private set; }

    /// <summary>Nome apenas para apresentação, sem utilização como caminho.</summary>
    public string? DisplayFileName { get; private set; }

    /// <summary>
    /// Limita apenas a apresentação.
    /// Nesta versão, o leitor ainda carrega o ficheiro completo em memória.
    /// </summary>
    public IReadOnlyList<CsvDataRecord> Preview =>
        Result?.Records.Take(20).ToArray()
        ?? Array.Empty<CsvDataRecord>();

    /// <summary>
    /// Problemas de leitura ou de estrutura encontrados no ficheiro completo.
    /// A pré-visualização continua limitada a vinte registos.
    /// </summary>
    public IReadOnlyList<ValidationIssue> Issues { get; private set; }
        = Array.Empty<ValidationIssue>();

    /// <summary>
    /// Determina a largura da tabela sem esconder colunas adicionais.
    /// </summary>
    public int PreviewColumnCount => Math.Max(
        Result?.Headers.Count ?? 0,
        Preview.Select(record => record.Values.Count)
            .DefaultIfEmpty(0)
            .Max());

    /// <summary>
    /// Indica se existe algum problema bloqueante no resultado apresentado.
    /// </summary>
    public bool HasErrors =>
        Issues.Any(issue => issue.Severity == ValidationSeverity.Error);

    /// <summary>
    /// Valida os parâmetros do envio e executa a leitura UTF-8.
    /// </summary>
    public IActionResult OnPost()
    {
        if (Upload is null || Upload.Length == 0)
        {
            ErrorKey = "ChooseNonEmptyFile";
            return Page();
        }

        if (Upload.Length > MaxFileSize)
        {
            ErrorKey = "FileTooLarge";
            return Page();
        }

        // Aceita apenas as opções disponíveis no formulário.
        if (Delimiter is not (";" or "," or "\t"))
        {
            ErrorKey = "InvalidDelimiter";
            return Page();
        }

        // Sem cabeçalho, não adivinha a quantidade esperada a partir dos dados.
        // Também rejeita texto não numérico ou valores fora do intervalo de int.
        if (!HasHeader &&
            (ExpectedColumnCount is null ||
             ExpectedColumnCount <= 0 ||
             (ModelState.TryGetValue(nameof(ExpectedColumnCount), out var entry)
              && entry.Errors.Count > 0)))
        {
            ErrorKey = "ExpectedColumnCountRequired";
            return Page();
        }

        DisplayFileName = Path.GetFileName(Upload.FileName);

        var encoding = new UTF8Encoding(
            encoderShouldEmitUTF8Identifier: true,
            throwOnInvalidBytes: true);

        try
        {
            using var stream = Upload.OpenReadStream();
            using var textReader = new StreamReader(
                stream,
                encoding,
                detectEncodingFromByteOrderMarks: false);

            Result = _reader.Read(textReader, Delimiter, HasHeader);

            if (!Result.IsComplete)
            {
                // Uma leitura interrompida não permite validar a estrutura completa.
                Issues = Result.ReadIssues;
            }
            else
            {
                // Um ficheiro vazio é encaminhado com uma quantidade positiva,
                // permitindo ao serviço devolver o diagnóstico de ficheiro vazio.
                int expectedCount = HasHeader
                    ? Math.Max(1, Result.Headers.Count)
                    : ExpectedColumnCount!.Value;

                // Sem mapeamentos, o serviço executa apenas a validação estrutural.
                Issues = _validator.Validate(
                    Result,
                    Array.Empty<FieldMapping>(),
                    expectedCount);
            }
        }
        catch (DecoderFallbackException)
        {
            // Descarta o resultado parcial quando os bytes não são UTF-8.
            ErrorKey = "InvalidEncoding";
        }
        catch (IOException)
        {
            // A mensagem operacional não expõe detalhes internos.
            ErrorKey = "ReadFailed";
        }

        return Page();
    }
}

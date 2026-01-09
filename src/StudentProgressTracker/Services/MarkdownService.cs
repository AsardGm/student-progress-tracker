using Markdig;

namespace StudentProgressTracker.Services;

/// <summary>
/// Rozhraní pro službu konvertující Markdown na HTML.
/// Používá se pro renderování zpráv AI a studentů v chatu.
/// </summary>
public interface IMarkdownService
{
    /// <summary>
    /// Konvertuje Markdown text na HTML.
    /// </summary>
    /// <param name="markdown">Vstupní Markdown text.</param>
    /// <returns>HTML výstup.</returns>
    string ToHtml(string markdown);
}

/// <summary>
/// Implementace služby pro konverzi Markdown na HTML.
/// Využívá knihovnu Markdig s rozšířeními pro tabulky, code blocks apod.
/// </summary>
public class MarkdownService : IMarkdownService
{
    private readonly MarkdownPipeline _pipeline;

    /// <summary>
    /// Inicializuje novou instanci MarkdownService.
    /// Konfiguruje Markdig pipeline s pokročilými rozšířeními.
    /// </summary>
    public MarkdownService()
    {
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()        // Tabulky, task listy, footnotes atd.
            .UseSoftlineBreakAsHardlineBreak() // Enter = nový řádek
            .Build();
    }

    /// <inheritdoc />
    public string ToHtml(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
            return string.Empty;

        return Markdown.ToHtml(markdown, _pipeline);
    }
}

using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using StudentProgressTracker.Configuration;
using StudentProgressTracker.Models;

namespace StudentProgressTracker.Services;

/// <summary>
/// Implementace AI služby využívající Google Gemini API.
/// Poskytuje analýzu zpráv studentů a generování personalizovaných odpovědí.
/// </summary>
public class GeminiAIService : IAIService
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly ILogger<GeminiAIService> _logger;

    /// <summary>
    /// Inicializuje novou instanci GeminiAIService.
    /// </summary>
    /// <param name="options">Konfigurace Gemini API (klíč, model, parametry).</param>
    /// <param name="httpClient">HTTP klient pro volání API.</param>
    /// <param name="logger">Logger pro diagnostiku.</param>
    /// <exception cref="InvalidOperationException">Pokud není nakonfigurován API klíč.</exception>
    public GeminiAIService(IOptions<GeminiOptions> options, HttpClient httpClient, ILogger<GeminiAIService> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;

        if (string.IsNullOrEmpty(_options.ApiKey))
        {
            throw new InvalidOperationException("Gemini API key is not configured. Set Gemini:ApiKey in configuration.");
        }
    }

    /// <inheritdoc />
    /// <remarks>
    /// Analýza zprávy vrací strukturovaný JSON s:
    /// - isRelevant: zda zpráva souvisí s cíli
    /// - relevantGoalIndex: index nejrelevantnějšího cíle
    /// - suggestedProgress: navrhovaný přírůstek pokroku
    /// - isOffTopic: zda je student mimo téma
    /// - needsHelp: zda potřebuje pomoc učitele
    /// </remarks>
    public async Task<AIAnalysis> AnalyzeMessageAsync(string message, List<Goal> goals, List<ChatMessage> recentMessages)
    {
        // Příprava kontextu cílů pro AI
        var goalsContext = string.Join("\n", goals.Select((g, i) =>
            $"{i + 1}. [{g.Type}] {g.Title} - {g.Description} (target: {g.TargetValue})"));

        // Příprava kontextu konverzace (posledních 10 zpráv)
        var recentContext = string.Join("\n", recentMessages.TakeLast(10).Select(m =>
            m.Type == MessageType.AI ? $"Asistent: {m.Content}" : $"Student: {m.Content}"));

        // Prompt pro strukturovanou analýzu v češtině
        var prompt = $@"Analyzuj zprávu studenta v kontextu jeho učebních cílů.

CÍLE:
{goalsContext}

NEDÁVNÁ KONVERZACE:
{recentContext}

AKTUÁLNÍ ZPRÁVA:
{message}

Odpověz ve formátu JSON (všechny textové hodnoty MUSÍ být česky):
{{
    ""isRelevant"": true/false (je zpráva relevantní k některému cíli),
    ""relevantGoalIndex"": číslo nebo null (1-based index nejrelevantnějšího cíle),
    ""suggestedProgress"": číslo nebo null (přírůstek pokroku 0-1),
    ""feedback"": ""krátká zpětná vazba pro studenta v češtině"",
    ""isOffTopic"": true/false (je student jasně mimo téma),
    ""summary"": ""krátké shrnutí česky co student udělal/zkusil"",
    ""needsHelp"": true/false (potřebuje student pomoc učitele?),
    ""helpReason"": ""důvod proč student potřebuje pomoc - ČESKY"" nebo null
}}

DŮLEŽITÉ - Nastav needsHelp=true pokud student:
- Explicitně žádá o pomoc nebo říká že je zaseknutý
- Vyjadřuje frustraci, zmatek nebo vzdává se
- Opakovaně chybuje na stejném konceptu
- Říká že nerozumí po více pokusech
- Zdá se být ztracený nebo přetížený

Buď povzbuzující ale přesný. Přidávej pokrok pouze za skutečné pokusy o řešení.";

        try
        {
            var response = await CallGeminiAsync(prompt);

            // Extrakce JSON z odpovědi (může obsahovat markdown formátování)
            var jsonStart = response.IndexOf('{');
            var jsonEnd = response.LastIndexOf('}');

            if (jsonStart >= 0 && jsonEnd > jsonStart)
            {
                var json = response.Substring(jsonStart, jsonEnd - jsonStart + 1);
                var result = JsonSerializer.Deserialize<GeminiAnalysisResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (result != null)
                {
                    return new AIAnalysis
                    {
                        IsRelevant = result.IsRelevant,
                        // Převod 1-based indexu na ID cíle
                        RelevantGoalId = result.RelevantGoalIndex.HasValue && result.RelevantGoalIndex.Value > 0 && result.RelevantGoalIndex.Value <= goals.Count
                            ? goals[result.RelevantGoalIndex.Value - 1].Id.ToString()
                            : null,
                        // Převod progress 0-1 na 0-100
                        SuggestedProgress = result.SuggestedProgress.HasValue ? (int)(result.SuggestedProgress.Value * 100) : null,
                        Feedback = result.Feedback,
                        IsOffTopic = result.IsOffTopic,
                        Summary = result.Summary,
                        NeedsHelp = result.NeedsHelp,
                        HelpReason = result.HelpReason
                    };
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to analyze message with Gemini");
        }

        // Fallback při selhání
        return new AIAnalysis { IsRelevant = false, IsOffTopic = false };
    }

    /// <inheritdoc />
    /// <remarks>
    /// Generuje odpověď jako přátelský učitelský asistent.
    /// Používá formátování: tučné, seznamy, LaTeX pro matematiku, code blocks.
    /// </remarks>
    public async Task<string> GenerateResponseAsync(string studentMessage, List<Goal> goals, List<MemberProgress> progress, List<ChatMessage> recentMessages)
    {
        // Příprava přehledu pokroku pro kontext
        var goalsWithProgress = goals.Select(g =>
        {
            var p = progress.FirstOrDefault(pr => pr.GoalId == g.Id);
            var progressText = g.Type == GoalType.Boolean
                ? (p?.IsCompleted == true ? "COMPLETED" : "NOT COMPLETED")
                : $"{p?.CurrentValue ?? 0}/{g.TargetValue}";
            return $"- {g.Title}: {progressText}";
        });

        var conversationHistory = string.Join("\n", recentMessages.TakeLast(10).Select(m =>
            m.Type == MessageType.AI ? $"Asistent: {m.Content}" : $"Student: {m.Content}"));

        // Systémový prompt definující chování AI asistenta
        var prompt = $@"Jsi ClassPulse AI - přátelský a trpělivý učitelský asistent. Pomáháš studentům s jejich učebními cíli.

## KONTEXT STUDENTA

### Cíle a pokrok:
{string.Join("\n", goalsWithProgress)}

### Historie konverzace:
{conversationHistory}

### Aktuální zpráva studenta:
{studentMessage}

## PRAVIDLA PRO ODPOVĚĎ

1. **Jazyk**: Vždy odpovídej česky, přirozeně a přátelsky.

2. **Formátování**:
   - Používej **tučné** pro důležité pojmy
   - Používej seznamy pro kroky nebo body
   - Pro matematiku používej LaTeX: $vzorec$ nebo $$blok$$
   - Pro kód používej `inline` nebo ```blok```

3. **Styl odpovědi**:
   - Buď povzbuzující, ale upřímný
   - Pokud student nerozumí, vysvětli jinak
   - Dávej nápovědy, ne hotová řešení
   - Ptej se na upřesnění, pokud je otázka nejasná

4. **Pokud je student mimo téma**:
   - Jemně ho přesměruj k úkolům
   - Připomeň aktuální cíle

5. **Délka**: Odpovědi by měly být stručné, ale užitečné (max 2-3 odstavce).

Odpověz na studentovu zprávu:";

        try
        {
            return await CallGeminiAsync(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate response with Gemini");
            return "Omlouvám se, momentálně nemohu odpovědět. Zkus to prosím znovu.";
        }
    }

    /// <inheritdoc />
    public async Task<string> GenerateWelcomeMessageAsync(Group group)
    {
        var goalsText = string.Join("\n", group.Goals.OrderBy(g => g.Order).Select((g, i) =>
        {
            var typeText = g.Type == GoalType.Boolean ? "splnit" : $"splnit {g.TargetValue}×";
            return $"{i + 1}. {g.Title} ({typeText})";
        }));

        var prompt = $@"Generate a friendly welcome message for a student joining a learning session.

SESSION: {group.Name}
DESCRIPTION: {group.Description}

GOALS TO COMPLETE:
{goalsText}

Create a welcoming message that:
1. Greets the student
2. Explains what they need to accomplish
3. Encourages them to start working
4. Mentions they can ask for help anytime

Keep it concise and friendly. Use Czech language.";

        try
        {
            return await CallGeminiAsync(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate welcome message");
            // Fallback uvítací zpráva
            return $"Vítej! Tvým cílem je: {group.Description}\n\nCíle:\n{goalsText}\n\nPokud potřebuješ pomoct, jen napiš!";
        }
    }

    /// <inheritdoc />
    public async Task<string> GenerateHintAsync(Goal goal, MemberProgress progress)
    {
        var progressText = goal.Type == GoalType.Boolean
            ? (progress.IsCompleted ? "splněno" : "nesplněno")
            : $"{progress.CurrentValue}/{goal.TargetValue}";

        var prompt = $@"Generate a helpful hint for a student working on this goal:

GOAL: {goal.Title}
DESCRIPTION: {goal.Description}
CURRENT PROGRESS: {progressText}

Provide a helpful hint that guides without giving away the answer.
Use Czech language. Keep it brief.";

        try
        {
            return await CallGeminiAsync(prompt);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate hint");
            return "Zkus se zamyslet nad zadáním a rozložit problém na menší části.";
        }
    }

    /// <summary>
    /// Volá Gemini API s daným promptem.
    /// </summary>
    /// <param name="prompt">Text promptu pro AI.</param>
    /// <param name="cancellationToken">Token pro zrušení operace.</param>
    /// <returns>Textová odpověď z API.</returns>
    private async Task<string> CallGeminiAsync(string prompt, CancellationToken cancellationToken = default)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_options.Model}:generateContent?key={_options.ApiKey}";

        var requestBody = new
        {
            contents = new[]
            {
                new
                {
                    parts = new[]
                    {
                        new { text = prompt }
                    }
                }
            },
            generationConfig = new
            {
                temperature = _options.Temperature,
                maxOutputTokens = _options.MaxOutputTokens
            }
        };

        var response = await _httpClient.PostAsJsonAsync(url, requestBody, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken);
        return result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? string.Empty;
    }

    #region Private DTOs pro deserializaci Gemini API odpovědí

    private class GeminiResponse
    {
        public List<Candidate>? Candidates { get; set; }
    }

    private class Candidate
    {
        public Content? Content { get; set; }
    }

    private class Content
    {
        public List<Part>? Parts { get; set; }
    }

    private class Part
    {
        public string? Text { get; set; }
    }

    /// <summary>
    /// DTO pro deserializaci strukturované AI analýzy.
    /// </summary>
    private class GeminiAnalysisResponse
    {
        public bool IsRelevant { get; set; }
        public int? RelevantGoalIndex { get; set; }
        public double? SuggestedProgress { get; set; }
        public string? Feedback { get; set; }
        public bool IsOffTopic { get; set; }
        public string? Summary { get; set; }
        public bool NeedsHelp { get; set; }
        public string? HelpReason { get; set; }
    }

    #endregion
}

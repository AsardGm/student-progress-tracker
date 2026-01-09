using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace StudentProgressTracker.Models;

/// <summary>
/// Reprezentuje zprávu v chatu skupiny.
/// Zprávy mohou být od studentů, systémové nebo od AI asistenta.
/// </summary>
public class ChatMessage
{
    /// <summary>
    /// Unikátní identifikátor zprávy.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Textový obsah zprávy.
    /// Může obsahovat Markdown formátování, LaTeX vzorce nebo kód.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Typ zprávy určující její původ a zobrazení.
    /// </summary>
    public MessageType Type { get; set; } = MessageType.User;

    /// <summary>
    /// Indikuje, zda byla zpráva označena jako relevantní k cílům.
    /// Relevantní zprávy jsou vizuálně zvýrazněny.
    /// </summary>
    public bool IsRelevantToGoal { get; set; }

    /// <summary>
    /// Navrhovaný přírůstek pokroku od AI (nullable).
    /// Např. 1 pokud AI usoudí, že student splnil jeden krok cíle.
    /// </summary>
    public int? ProgressContribution { get; set; }

    /// <summary>
    /// JSON serializace AI analýzy zprávy.
    /// Ukládá se do databáze jako string.
    /// </summary>
    public string? AIAnalysisJson { get; set; }

    /// <summary>
    /// Indikuje, zda je zpráva varováním (nečinnost, off-topic).
    /// Varování jsou vizuálně odlišena.
    /// </summary>
    public bool IsWarning { get; set; }

    /// <summary>
    /// Časové razítko odeslání zprávy (UTC).
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ID skupiny, do které zpráva patří.
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// ID člena, který zprávu odeslal (nullable pro systémové/AI zprávy).
    /// </summary>
    public Guid? MemberId { get; set; }

    /// <summary>
    /// Navigační vlastnost na skupinu.
    /// </summary>
    public Group Group { get; set; } = null!;

    /// <summary>
    /// Navigační vlastnost na člena (nullable pro systémové/AI zprávy).
    /// </summary>
    public GroupMember? Member { get; set; }

    /// <summary>
    /// Vypočítaná vlastnost pro přístup k AI analýze jako objektu.
    /// Automaticky serializuje/deserializuje z/do AIAnalysisJson.
    /// Není mapována do databáze (NotMapped).
    /// </summary>
    [NotMapped]
    public AIAnalysis? AIAnalysis
    {
        get => string.IsNullOrEmpty(AIAnalysisJson)
            ? null
            : JsonSerializer.Deserialize<AIAnalysis>(AIAnalysisJson);
        set => AIAnalysisJson = value == null
            ? null
            : JsonSerializer.Serialize(value);
    }
}

/// <summary>
/// Definuje typ zprávy v chatu.
/// </summary>
public enum MessageType
{
    /// <summary>Zpráva od studenta.</summary>
    User,

    /// <summary>Systémová zpráva (varování o nečinnosti apod.).</summary>
    System,

    /// <summary>Odpověď od AI asistenta (Gemini).</summary>
    AI
}

/// <summary>
/// Výsledek AI analýzy zprávy studenta.
/// Obsahuje hodnocení relevance, pokroku a případné detekce problémů.
/// Není mapována do databáze - ukládá se jako JSON v AIAnalysisJson.
/// </summary>
[NotMapped]
public class AIAnalysis
{
    /// <summary>
    /// Indikuje, zda je zpráva relevantní k některému z cílů.
    /// </summary>
    public bool IsRelevant { get; set; }

    /// <summary>
    /// ID cíle, ke kterému je zpráva relevantní (GUID jako string).
    /// </summary>
    public string? RelevantGoalId { get; set; }

    /// <summary>
    /// Navrhovaný přírůstek pokroku pro daný cíl.
    /// </summary>
    public int? SuggestedProgress { get; set; }

    /// <summary>
    /// Textová zpětná vazba od AI pro studenta.
    /// </summary>
    public string? Feedback { get; set; }

    /// <summary>
    /// Indikuje, zda je zpráva mimo téma (off-topic).
    /// </summary>
    public bool IsOffTopic { get; set; }

    /// <summary>
    /// Shrnutí zprávy pro učitele.
    /// </summary>
    public string? Summary { get; set; }

    /// <summary>
    /// Indikuje, zda AI detekovala, že student potřebuje pomoc.
    /// </summary>
    public bool NeedsHelp { get; set; }

    /// <summary>
    /// Důvod proč student potřebuje pomoc (v češtině).
    /// </summary>
    public string? HelpReason { get; set; }
}

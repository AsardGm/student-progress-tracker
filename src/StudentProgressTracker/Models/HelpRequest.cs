namespace StudentProgressTracker.Models;

/// <summary>
/// Reprezentuje žádost studenta o pomoc učitele.
/// Žádosti mohou být vytvořeny manuálně studentem nebo automaticky systémem/AI.
/// </summary>
public class HelpRequest
{
    /// <summary>
    /// Unikátní identifikátor žádosti o pomoc.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Důvod žádosti o pomoc zobrazovaný učiteli.
    /// </summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>
    /// Typ žádosti určující její původ.
    /// </summary>
    public HelpRequestType Type { get; set; } = HelpRequestType.Manual;

    /// <summary>
    /// Datum a čas vytvoření žádosti (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Datum a čas vyřešení žádosti učitelem (nullable dokud není vyřešena).
    /// </summary>
    public DateTime? ResolvedAt { get; set; }

    /// <summary>
    /// ID člena (studenta), který žádá o pomoc.
    /// </summary>
    public Guid MemberId { get; set; }

    /// <summary>
    /// ID učitele, který žádost vyřešil (nullable dokud není vyřešena).
    /// </summary>
    public string? ResolvedById { get; set; }

    /// <summary>
    /// Navigační vlastnost na člena skupiny.
    /// </summary>
    public GroupMember Member { get; set; } = null!;

    /// <summary>
    /// Navigační vlastnost na učitele, který vyřešil žádost.
    /// </summary>
    public User? ResolvedBy { get; set; }
}

/// <summary>
/// Definuje typ/původ žádosti o pomoc.
/// Umožňuje rozlišit manuální žádosti od automaticky detekovaných.
/// </summary>
public enum HelpRequestType
{
    /// <summary>
    /// Manuální žádost - student klikl na tlačítko "Potřebuji pomoc".
    /// </summary>
    Manual,

    /// <summary>
    /// Automatická detekce nečinnosti - student dlouho nepsal.
    /// </summary>
    Inactivity,

    /// <summary>
    /// Automatická detekce off-topic - student píše mimo téma.
    /// </summary>
    OffTopic,

    /// <summary>
    /// AI detekce - Gemini rozpoznal, že student potřebuje pomoc.
    /// </summary>
    AIDetected
}

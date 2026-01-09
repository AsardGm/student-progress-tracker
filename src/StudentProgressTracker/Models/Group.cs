namespace StudentProgressTracker.Models;

/// <summary>
/// Reprezentuje studijní skupinu vytvořenou učitelem.
/// Skupina obsahuje cíle, členy (studenty) a chat pro komunikaci s AI.
/// </summary>
public class Group
{
    /// <summary>
    /// Unikátní identifikátor skupiny.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Název skupiny zobrazovaný v UI.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Volitelný popis skupiny s detaily o úkolu.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Krátký kód pro připojení do skupiny (8 znaků).
    /// Studenti tento kód zadávají nebo skenují z QR kódu.
    /// </summary>
    public string JoinCode { get; set; } = string.Empty;

    /// <summary>
    /// QR kód zakódovaný jako Base64 PNG obrázek.
    /// Obsahuje URL pro připojení do skupiny.
    /// </summary>
    public string? QRCodeBase64 { get; set; }

    /// <summary>
    /// Indikuje, zda je skupina aktivní.
    /// Neaktivní skupiny neumožňují nové členy ani chat.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Datum a čas vytvoření skupiny (UTC).
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ID vlastníka (učitele), který skupinu vytvořil.
    /// </summary>
    public string OwnerId { get; set; } = string.Empty;

    /// <summary>
    /// Navigační vlastnost na vlastníka skupiny.
    /// </summary>
    public User Owner { get; set; } = null!;

    /// <summary>
    /// Kolekce cílů definovaných pro tuto skupinu.
    /// </summary>
    public ICollection<Goal> Goals { get; set; } = new List<Goal>();

    /// <summary>
    /// Kolekce členů (studentů) přihlášených do skupiny.
    /// </summary>
    public ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();

    /// <summary>
    /// Kolekce všech chatových zpráv ve skupině.
    /// </summary>
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
}

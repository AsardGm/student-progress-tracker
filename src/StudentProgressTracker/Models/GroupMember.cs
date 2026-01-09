namespace StudentProgressTracker.Models;

/// <summary>
/// Reprezentuje člena (studenta) ve skupině.
/// Studenti se nepřihlašují - jsou identifikováni pomocí DeviceId a přezdívky.
/// </summary>
public class GroupMember
{
    /// <summary>
    /// Unikátní identifikátor člena.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Unikátní identifikátor zařízení z localStorage.
    /// Umožňuje rozpoznat stejného studenta při opětovném připojení.
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// Přezdívka studenta zobrazovaná v chatu a přehledu.
    /// Student si ji volí při připojení do skupiny.
    /// </summary>
    public string Nickname { get; set; } = string.Empty;

    /// <summary>
    /// Indikuje, že student aktuálně potřebuje pomoc učitele.
    /// Nastavuje se manuálně studentem nebo automaticky AI/systémem.
    /// </summary>
    public bool NeedsHelp { get; set; }

    /// <summary>
    /// Důvod proč student potřebuje pomoc.
    /// Zobrazuje se učiteli v přehledu skupiny.
    /// </summary>
    public string? HelpReason { get; set; }

    /// <summary>
    /// Čas poslední aktivity studenta (odeslání zprávy).
    /// Používá se pro detekci nečinnosti.
    /// </summary>
    public DateTime? LastActiveAt { get; set; }

    /// <summary>
    /// Datum a čas připojení do skupiny (UTC).
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ID skupiny, ve které je student členem.
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// Navigační vlastnost na skupinu.
    /// </summary>
    public Group Group { get; set; } = null!;

    /// <summary>
    /// Kolekce pokroků studenta u jednotlivých cílů.
    /// </summary>
    public ICollection<MemberProgress> Progresses { get; set; } = new List<MemberProgress>();

    /// <summary>
    /// Kolekce chatových zpráv odeslaných studentem.
    /// </summary>
    public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

    /// <summary>
    /// Kolekce žádostí o pomoc vytvořených pro tohoto studenta.
    /// </summary>
    public ICollection<HelpRequest> HelpRequests { get; set; } = new List<HelpRequest>();
}

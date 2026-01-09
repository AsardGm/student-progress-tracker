using StudentProgressTracker.Models;

namespace StudentProgressTracker.Services;

/// <summary>
/// Rozhraní pro službu sledující pokrok studentů a žádosti o pomoc.
/// Zajišťuje aktualizaci pokroku, detekci nečinnosti a správu help requestů.
/// </summary>
public interface IProgressService
{
    /// <summary>
    /// Získá pokrok člena u konkrétního cíle.
    /// </summary>
    /// <param name="memberId">ID člena.</param>
    /// <param name="goalId">ID cíle.</param>
    /// <returns>Záznam pokroku nebo null.</returns>
    Task<MemberProgress?> GetProgressAsync(Guid memberId, Guid goalId);

    /// <summary>
    /// Získá všechny pokroky člena seřazené podle pořadí cílů.
    /// </summary>
    /// <param name="memberId">ID člena.</param>
    /// <returns>Seznam pokroků u všech cílů.</returns>
    Task<List<MemberProgress>> GetMemberProgressesAsync(Guid memberId);

    /// <summary>
    /// Aktualizuje hodnotu pokroku u cíle.
    /// </summary>
    /// <param name="memberId">ID člena.</param>
    /// <param name="goalId">ID cíle.</param>
    /// <param name="newValue">Nová hodnota pokroku.</param>
    /// <param name="isCompleted">Zda je cíl splněn.</param>
    Task UpdateProgressAsync(Guid memberId, Guid goalId, int newValue, bool isCompleted);

    /// <summary>
    /// Inkrementuje pokrok u procentuálního cíle.
    /// Hodnota je omezena na maximum TargetValue.
    /// </summary>
    /// <param name="memberId">ID člena.</param>
    /// <param name="goalId">ID cíle.</param>
    /// <param name="increment">Hodnota přírůstku (výchozí 1).</param>
    Task IncrementProgressAsync(Guid memberId, Guid goalId, int increment = 1);

    /// <summary>
    /// Nastaví stav splnění binárního cíle.
    /// </summary>
    /// <param name="memberId">ID člena.</param>
    /// <param name="goalId">ID cíle.</param>
    /// <param name="completed">True pokud splněno.</param>
    Task SetCompletedAsync(Guid memberId, Guid goalId, bool completed);

    /// <summary>
    /// Aktualizuje čas poslední aktivity člena.
    /// </summary>
    /// <param name="memberId">ID člena.</param>
    Task UpdateLastActiveAsync(Guid memberId);

    /// <summary>
    /// Získá seznam neaktivních členů ve skupině.
    /// </summary>
    /// <param name="groupId">ID skupiny.</param>
    /// <param name="threshold">Práh nečinnosti.</param>
    /// <returns>Seznam neaktivních členů.</returns>
    Task<List<GroupMember>> GetInactiveMembersAsync(Guid groupId, TimeSpan threshold);

    /// <summary>
    /// Nastaví příznak potřeby pomoci u člena.
    /// </summary>
    /// <param name="memberId">ID člena.</param>
    /// <param name="needsHelp">True pokud potřebuje pomoc.</param>
    /// <param name="reason">Volitelný důvod.</param>
    Task SetNeedsHelpAsync(Guid memberId, bool needsHelp, string? reason = null);

    /// <summary>
    /// Vytvoří žádost o pomoc pro člena.
    /// Automaticky nastaví NeedsHelp příznak.
    /// </summary>
    /// <param name="memberId">ID člena.</param>
    /// <param name="reason">Důvod žádosti.</param>
    /// <param name="type">Typ žádosti (manuální, nečinnost, AI detekce).</param>
    /// <returns>Vytvořená žádost.</returns>
    Task<HelpRequest> CreateHelpRequestAsync(Guid memberId, string reason, HelpRequestType type);

    /// <summary>
    /// Označí žádost o pomoc jako vyřešenou.
    /// Automaticky vymaže NeedsHelp příznak.
    /// </summary>
    /// <param name="requestId">ID žádosti.</param>
    /// <param name="resolvedById">ID učitele, který vyřešil.</param>
    Task ResolveHelpRequestAsync(Guid requestId, string resolvedById);

    /// <summary>
    /// Získá aktivní (nevyřešené) žádosti o pomoc ve skupině.
    /// </summary>
    /// <param name="groupId">ID skupiny.</param>
    /// <returns>Seznam aktivních žádostí.</returns>
    Task<List<HelpRequest>> GetActiveHelpRequestsAsync(Guid groupId);
}

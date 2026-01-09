using StudentProgressTracker.Models;

namespace StudentProgressTracker.Services;

/// <summary>
/// Rozhraní pro službu spravující chat zprávy ve skupinách.
/// Zajišťuje odesílání zpráv, načítání historie a aktualizaci AI analýzy.
/// </summary>
public interface IChatService
{
    /// <summary>
    /// Odešle zprávu od studenta do skupiny.
    /// Automaticky aktualizuje čas poslední aktivity studenta.
    /// </summary>
    /// <param name="groupId">ID skupiny.</param>
    /// <param name="memberId">ID člena (studenta).</param>
    /// <param name="content">Obsah zprávy.</param>
    /// <returns>Vytvořená zpráva.</returns>
    Task<ChatMessage> SendMessageAsync(Guid groupId, Guid memberId, string content);

    /// <summary>
    /// Odešle systémovou zprávu do skupiny (varování, notifikace).
    /// </summary>
    /// <param name="groupId">ID skupiny.</param>
    /// <param name="content">Obsah systémové zprávy.</param>
    /// <returns>Vytvořená zpráva.</returns>
    Task<ChatMessage> SendSystemMessageAsync(Guid groupId, string content);

    /// <summary>
    /// Odešle odpověď AI asistenta jako reakci na zprávu studenta.
    /// </summary>
    /// <param name="groupId">ID skupiny.</param>
    /// <param name="memberId">ID člena, kterému AI odpovídá.</param>
    /// <param name="content">Obsah AI odpovědi.</param>
    /// <returns>Vytvořená zpráva.</returns>
    Task<ChatMessage> SendAIMessageAsync(Guid groupId, Guid memberId, string content);

    /// <summary>
    /// Získá historii zpráv skupiny nebo konkrétního člena.
    /// </summary>
    /// <param name="groupId">ID skupiny.</param>
    /// <param name="memberId">Volitelné ID člena pro filtrování.</param>
    /// <param name="take">Maximální počet zpráv.</param>
    /// <returns>Seznam zpráv seřazených od nejstarší.</returns>
    Task<List<ChatMessage>> GetMessagesAsync(Guid groupId, Guid? memberId = null, int take = 100);

    /// <summary>
    /// Získá klíčové zprávy člena (relevantní k cílům).
    /// Používá se pro zobrazení přehledu učiteli.
    /// </summary>
    /// <param name="memberId">ID člena.</param>
    /// <returns>Seznam relevantních zpráv.</returns>
    Task<List<ChatMessage>> GetKeyMessagesAsync(Guid memberId);

    /// <summary>
    /// Aktualizuje AI analýzu zprávy.
    /// Nastavuje příznaky relevance, pokroku a varování.
    /// </summary>
    /// <param name="messageId">ID zprávy.</param>
    /// <param name="analysis">Výsledek AI analýzy.</param>
    Task UpdateMessageAnalysisAsync(Guid messageId, AIAnalysis analysis);
}

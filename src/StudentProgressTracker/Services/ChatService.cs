using Microsoft.EntityFrameworkCore;
using StudentProgressTracker.Data;
using StudentProgressTracker.Models;

namespace StudentProgressTracker.Services;

/// <summary>
/// Implementace služby pro správu chat zpráv.
/// Zajišťuje persistenci zpráv a sledování aktivity studentů.
/// </summary>
public class ChatService : IChatService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ChatService> _logger;

    /// <summary>
    /// Inicializuje novou instanci ChatService.
    /// </summary>
    /// <param name="context">Databázový kontext.</param>
    /// <param name="logger">Logger pro diagnostiku.</param>
    public ChatService(ApplicationDbContext context, ILogger<ChatService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<ChatMessage> SendMessageAsync(Guid groupId, Guid memberId, string content)
    {
        var message = new ChatMessage
        {
            GroupId = groupId,
            MemberId = memberId,
            Content = content,
            Type = MessageType.User,
            Timestamp = DateTime.UtcNow
        };

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        // Aktualizace času poslední aktivity studenta
        var member = await _context.GroupMembers.FindAsync(memberId);
        if (member != null)
        {
            member.LastActiveAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        return message;
    }

    /// <inheritdoc />
    public async Task<ChatMessage> SendSystemMessageAsync(Guid groupId, string content)
    {
        var message = new ChatMessage
        {
            GroupId = groupId,
            MemberId = null,  // Systémové zprávy nemají autora
            Content = content,
            Type = MessageType.System,
            Timestamp = DateTime.UtcNow
        };

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        return message;
    }

    /// <inheritdoc />
    public async Task<ChatMessage> SendAIMessageAsync(Guid groupId, Guid memberId, string content)
    {
        var message = new ChatMessage
        {
            GroupId = groupId,
            MemberId = memberId,  // AI odpovídá konkrétnímu studentovi
            Content = content,
            Type = MessageType.AI,
            Timestamp = DateTime.UtcNow
        };

        _context.ChatMessages.Add(message);
        await _context.SaveChangesAsync();

        return message;
    }

    /// <inheritdoc />
    public async Task<List<ChatMessage>> GetMessagesAsync(Guid groupId, Guid? memberId = null, int take = 100)
    {
        var query = _context.ChatMessages
            .Include(m => m.Member)
            .Where(m => m.GroupId == groupId);

        if (memberId.HasValue)
        {
            // Pro konkrétního člena: jeho zprávy + systémové + AI odpovědi
            query = query.Where(m => m.MemberId == memberId || m.MemberId == null || m.Type == MessageType.AI);
        }

        // Nejprve seřadíme sestupně pro Take, pak vzestupně pro zobrazení
        return await query
            .OrderByDescending(m => m.Timestamp)
            .Take(take)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<List<ChatMessage>> GetKeyMessagesAsync(Guid memberId)
    {
        // Pouze zprávy označené jako relevantní k cílům
        return await _context.ChatMessages
            .Include(m => m.Member)
            .Where(m => m.MemberId == memberId && m.IsRelevantToGoal)
            .OrderBy(m => m.Timestamp)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task UpdateMessageAnalysisAsync(Guid messageId, AIAnalysis analysis)
    {
        var message = await _context.ChatMessages.FindAsync(messageId);
        if (message != null)
        {
            // Uložení kompletní AI analýzy jako JSON
            message.AIAnalysis = analysis;
            // Extrakce klíčových příznaků pro rychlé filtrování
            message.IsRelevantToGoal = analysis.IsRelevant;
            message.ProgressContribution = analysis.SuggestedProgress;
            message.IsWarning = analysis.IsOffTopic;
            await _context.SaveChangesAsync();
        }
    }
}

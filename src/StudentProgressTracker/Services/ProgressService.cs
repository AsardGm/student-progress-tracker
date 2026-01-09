using Microsoft.EntityFrameworkCore;
using StudentProgressTracker.Data;
using StudentProgressTracker.Models;

namespace StudentProgressTracker.Services;

/// <summary>
/// Implementace služby pro sledování pokroku studentů.
/// Zajišťuje aktualizaci pokroku, detekci nečinnosti a správu žádostí o pomoc.
/// </summary>
public class ProgressService : IProgressService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ProgressService> _logger;

    /// <summary>
    /// Inicializuje novou instanci ProgressService.
    /// </summary>
    /// <param name="context">Databázový kontext.</param>
    /// <param name="logger">Logger pro diagnostiku.</param>
    public ProgressService(ApplicationDbContext context, ILogger<ProgressService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<MemberProgress?> GetProgressAsync(Guid memberId, Guid goalId)
    {
        return await _context.MemberProgresses
            .Include(p => p.Goal)
            .FirstOrDefaultAsync(p => p.MemberId == memberId && p.GoalId == goalId);
    }

    /// <inheritdoc />
    public async Task<List<MemberProgress>> GetMemberProgressesAsync(Guid memberId)
    {
        return await _context.MemberProgresses
            .Include(p => p.Goal)
            .Where(p => p.MemberId == memberId)
            .OrderBy(p => p.Goal.Order)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task UpdateProgressAsync(Guid memberId, Guid goalId, int newValue, bool isCompleted)
    {
        var progress = await _context.MemberProgresses
            .Include(p => p.Goal)
            .FirstOrDefaultAsync(p => p.MemberId == memberId && p.GoalId == goalId);

        if (progress != null)
        {
            progress.CurrentValue = newValue;
            progress.IsCompleted = isCompleted;
            progress.UpdatedAt = DateTime.UtcNow;

            // Automatické dokončení při dosažení cílové hodnoty
            if (progress.Goal.Type == GoalType.Percentage && newValue >= progress.Goal.TargetValue)
            {
                progress.IsCompleted = true;
            }

            await _context.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task IncrementProgressAsync(Guid memberId, Guid goalId, int increment = 1)
    {
        var progress = await _context.MemberProgresses
            .Include(p => p.Goal)
            .FirstOrDefaultAsync(p => p.MemberId == memberId && p.GoalId == goalId);

        if (progress != null)
        {
            // Hodnota nemůže překročit TargetValue
            progress.CurrentValue = Math.Min(progress.CurrentValue + increment, progress.Goal.TargetValue);
            progress.UpdatedAt = DateTime.UtcNow;

            // Automatické dokončení při dosažení cílové hodnoty
            if (progress.Goal.Type == GoalType.Percentage && progress.CurrentValue >= progress.Goal.TargetValue)
            {
                progress.IsCompleted = true;
            }

            await _context.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task SetCompletedAsync(Guid memberId, Guid goalId, bool completed)
    {
        var progress = await _context.MemberProgresses
            .FirstOrDefaultAsync(p => p.MemberId == memberId && p.GoalId == goalId);

        if (progress != null)
        {
            progress.IsCompleted = completed;
            progress.UpdatedAt = DateTime.UtcNow;
            // Pro binární cíle nastavíme CurrentValue na 1 při splnění
            if (completed)
            {
                progress.CurrentValue = 1;
            }
            await _context.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task UpdateLastActiveAsync(Guid memberId)
    {
        var member = await _context.GroupMembers.FindAsync(memberId);
        if (member != null)
        {
            member.LastActiveAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task<List<GroupMember>> GetInactiveMembersAsync(Guid groupId, TimeSpan threshold)
    {
        // Výpočet hranice nečinnosti
        var cutoff = DateTime.UtcNow - threshold;
        return await _context.GroupMembers
            .Where(m => m.GroupId == groupId && m.LastActiveAt < cutoff)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task SetNeedsHelpAsync(Guid memberId, bool needsHelp, string? reason = null)
    {
        var member = await _context.GroupMembers.FindAsync(memberId);
        if (member != null)
        {
            member.NeedsHelp = needsHelp;
            member.HelpReason = reason;
            await _context.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task<HelpRequest> CreateHelpRequestAsync(Guid memberId, string reason, HelpRequestType type)
    {
        var request = new HelpRequest
        {
            MemberId = memberId,
            Reason = reason,
            Type = type,
            CreatedAt = DateTime.UtcNow
        };

        _context.HelpRequests.Add(request);

        // Automatické nastavení příznaku potřeby pomoci na členovi
        var member = await _context.GroupMembers.FindAsync(memberId);
        if (member != null)
        {
            member.NeedsHelp = true;
            member.HelpReason = reason;
        }

        await _context.SaveChangesAsync();
        return request;
    }

    /// <inheritdoc />
    public async Task ResolveHelpRequestAsync(Guid requestId, string resolvedById)
    {
        var request = await _context.HelpRequests
            .Include(r => r.Member)
            .FirstOrDefaultAsync(r => r.Id == requestId);

        if (request != null)
        {
            // Označení žádosti jako vyřešené
            request.ResolvedAt = DateTime.UtcNow;
            request.ResolvedById = resolvedById;

            // Vymazání příznaku potřeby pomoci na členovi
            request.Member.NeedsHelp = false;
            request.Member.HelpReason = null;

            await _context.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task<List<HelpRequest>> GetActiveHelpRequestsAsync(Guid groupId)
    {
        // Nevyřešené žádosti (ResolvedAt == null) seřazené od nejnovější
        return await _context.HelpRequests
            .Include(r => r.Member)
            .Where(r => r.Member.GroupId == groupId && r.ResolvedAt == null)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }
}

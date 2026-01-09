using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using StudentProgressTracker.Data;
using StudentProgressTracker.Models;

namespace StudentProgressTracker.Services;

/// <summary>
/// Implementace služby pro správu skupin.
/// Zajišťuje vytváření skupin, generování QR kódů a správu členů.
/// </summary>
public class GroupService : IGroupService
{
    private readonly ApplicationDbContext _context;
    private readonly IQRCodeService _qrCodeService;
    private readonly ILogger<GroupService> _logger;

    /// <summary>
    /// Inicializuje novou instanci GroupService.
    /// </summary>
    /// <param name="context">Databázový kontext.</param>
    /// <param name="qrCodeService">Služba pro generování QR kódů.</param>
    /// <param name="logger">Logger pro diagnostiku.</param>
    public GroupService(ApplicationDbContext context, IQRCodeService qrCodeService, ILogger<GroupService> logger)
    {
        _context = context;
        _qrCodeService = qrCodeService;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Group> CreateGroupAsync(string name, string description, List<GoalDto> goals, string ownerId)
    {
        // Validace vstupů
        ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
        ArgumentException.ThrowIfNullOrWhiteSpace(ownerId, nameof(ownerId));
        ArgumentNullException.ThrowIfNull(goals, nameof(goals));

        if (goals.Count == 0)
            throw new ArgumentException("At least one goal is required.", nameof(goals));

        _logger.LogInformation("Creating group '{GroupName}' for owner {OwnerId}", name, ownerId);

        // Generování unikátního přístupového kódu
        var joinCode = GenerateJoinCode();

        var group = new Group
        {
            Name = name.Trim(),
            Description = description?.Trim() ?? string.Empty,
            JoinCode = joinCode,
            OwnerId = ownerId,
            CreatedAt = DateTime.UtcNow
        };

        // Generování QR kódu s URL pro připojení
        group.QRCodeBase64 = _qrCodeService.GenerateQRCode($"/join/{joinCode}");

        // Přidání cílů s pořadím
        for (int i = 0; i < goals.Count; i++)
        {
            var goalDto = goals[i];
            group.Goals.Add(new Goal
            {
                Title = goalDto.Title,
                Description = goalDto.Description,
                Type = goalDto.Type,
                TargetValue = goalDto.TargetValue,
                Order = i
            });
        }

        _context.Groups.Add(group);
        await _context.SaveChangesAsync();

        return group;
    }

    /// <inheritdoc />
    public async Task<Group?> GetGroupAsync(Guid groupId)
    {
        // Eager loading všech souvisejících entit pro zobrazení detailu
        return await _context.Groups
            .Include(g => g.Owner)
            .Include(g => g.Goals.OrderBy(gl => gl.Order))
            .Include(g => g.Members)
                .ThenInclude(m => m.Progresses)
            .FirstOrDefaultAsync(g => g.Id == groupId);
    }

    /// <inheritdoc />
    public async Task<Group?> GetGroupByJoinCodeAsync(string joinCode)
    {
        // Vrací pouze aktivní skupiny
        return await _context.Groups
            .Include(g => g.Goals.OrderBy(gl => gl.Order))
            .FirstOrDefaultAsync(g => g.JoinCode == joinCode && g.IsActive);
    }

    /// <inheritdoc />
    public async Task<List<Group>> GetGroupsByOwnerAsync(string ownerId)
    {
        return await _context.Groups
            .Include(g => g.Goals)
            .Include(g => g.Members)
            .Where(g => g.OwnerId == ownerId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<GroupMember> JoinGroupAsync(Guid groupId, string deviceId, string nickname)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(deviceId, nameof(deviceId));
        ArgumentException.ThrowIfNullOrWhiteSpace(nickname, nameof(nickname));

        if (groupId == Guid.Empty)
            throw new ArgumentException("Invalid group ID.", nameof(groupId));

        _logger.LogInformation("Student '{Nickname}' joining group {GroupId}", nickname, groupId);

        var member = new GroupMember
        {
            GroupId = groupId,
            DeviceId = deviceId,
            Nickname = nickname,
            JoinedAt = DateTime.UtcNow,
            LastActiveAt = DateTime.UtcNow
        };

        _context.GroupMembers.Add(member);

        // Inicializace pokroku pro všechny cíle skupiny
        var goals = await _context.Goals.Where(g => g.GroupId == groupId).ToListAsync();
        foreach (var goal in goals)
        {
            member.Progresses.Add(new MemberProgress
            {
                MemberId = member.Id,
                GoalId = goal.Id,
                CurrentValue = 0,
                IsCompleted = false
            });
        }

        await _context.SaveChangesAsync();
        return member;
    }

    /// <inheritdoc />
    public async Task<GroupMember?> GetMemberByDeviceIdAsync(Guid groupId, string deviceId)
    {
        return await _context.GroupMembers
            .Include(m => m.Progresses)
                .ThenInclude(p => p.Goal)
            .Include(m => m.Messages.OrderByDescending(msg => msg.Timestamp).Take(50))
            .FirstOrDefaultAsync(m => m.GroupId == groupId && m.DeviceId == deviceId);
    }

    /// <inheritdoc />
    public async Task<List<GroupMember>> GetGroupMembersAsync(Guid groupId)
    {
        return await _context.GroupMembers
            .Include(m => m.Progresses)
                .ThenInclude(p => p.Goal)
            .Where(m => m.GroupId == groupId)
            .OrderBy(m => m.Nickname)
            .ToListAsync();
    }

    /// <inheritdoc />
    public async Task<bool> HasDeviceJoinedGroupAsync(Guid groupId, string deviceId)
    {
        return await _context.GroupMembers
            .AnyAsync(m => m.GroupId == groupId && m.DeviceId == deviceId);
    }

    /// <inheritdoc />
    public async Task UpdateGroupAsync(Group group)
    {
        _context.Groups.Update(group);
        await _context.SaveChangesAsync();
    }

    /// <inheritdoc />
    public async Task DeleteGroupAsync(Guid groupId)
    {
        var group = await _context.Groups.FindAsync(groupId);
        if (group != null)
        {
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
        }
    }

    /// <inheritdoc />
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }

    /// <summary>
    /// Generuje náhodný 6-znakový přístupový kód.
    /// Používá kryptograficky bezpečný generátor náhodných čísel.
    /// Vynechává snadno zaměnitelné znaky (0, O, I, 1).
    /// </summary>
    /// <returns>6-znakový kód složený z velkých písmen a číslic.</returns>
    private static string GenerateJoinCode()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        const int codeLength = 6;

        Span<byte> randomBytes = stackalloc byte[codeLength];
        RandomNumberGenerator.Fill(randomBytes);

        return string.Create(codeLength, randomBytes.ToArray(), (span, bytes) =>
        {
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = chars[bytes[i] % chars.Length];
            }
        });
    }
}

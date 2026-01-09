using Microsoft.AspNetCore.SignalR;
using StudentProgressTracker.Hubs;
using StudentProgressTracker.Models;

namespace StudentProgressTracker.Services;

/// <summary>
/// Rozhraní pro službu odesílající real-time notifikace přes SignalR.
/// Poskytuje abstrakci nad ProgressHub pro použití ve službách.
/// </summary>
public interface IProgressHubService
{
    /// <summary>
    /// Notifikuje o nové zprávě v chatu.
    /// Odesílá všem členům skupiny i učiteli.
    /// </summary>
    Task NotifyMessageSent(Guid groupId, ChatMessage message);

    /// <summary>
    /// Notifikuje o změně pokroku studenta.
    /// </summary>
    Task NotifyProgressUpdated(Guid groupId, GroupMember member, MemberProgress progress);

    /// <summary>
    /// Notifikuje učitele o připojení nového studenta.
    /// </summary>
    Task NotifyMemberJoined(Guid groupId, GroupMember member);

    /// <summary>
    /// Notifikuje učitele o žádosti o pomoc.
    /// </summary>
    Task NotifyMemberNeedsHelp(Guid groupId, HelpRequest request, string memberNickname);

    /// <summary>
    /// Notifikuje učitele o manuální žádosti o pomoc.
    /// </summary>
    Task NotifyHelpRequested(Guid groupId, Guid memberId, string reason);

    /// <summary>
    /// Notifikuje o vyřešení žádosti o pomoc.
    /// </summary>
    Task NotifyHelpResolved(Guid groupId, Guid memberId);

    /// <summary>
    /// Notifikuje o změně aktivity studenta (aktivní/neaktivní).
    /// </summary>
    Task NotifyMemberActivityChanged(Guid groupId, Guid memberId, bool isActive);
}

/// <summary>
/// Implementace služby pro real-time notifikace přes SignalR.
/// Používá IHubContext pro odesílání zpráv klientům.
/// </summary>
public class ProgressHubService : IProgressHubService
{
    private readonly IHubContext<ProgressHub, IProgressHubClient> _hubContext;

    /// <summary>
    /// Inicializuje novou instanci ProgressHubService.
    /// </summary>
    /// <param name="hubContext">SignalR hub context pro ProgressHub.</param>
    public ProgressHubService(IHubContext<ProgressHub, IProgressHubClient> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <inheritdoc />
    public async Task NotifyMessageSent(Guid groupId, ChatMessage message)
    {
        var dto = ChatMessageDto.FromEntity(message);

        // Odeslání všem členům ve skupině (studenti)
        await _hubContext.Clients.Group(groupId.ToString()).ReceiveMessage(dto);

        // Odeslání do učitelského dashboardu (oddělená skupina)
        await _hubContext.Clients.Group($"teacher-{groupId}").ReceiveMessage(dto);
    }

    /// <inheritdoc />
    public async Task NotifyProgressUpdated(Guid groupId, GroupMember member, MemberProgress progress)
    {
        // Vytvoření DTO s kompletními informacemi o pokroku
        var dto = new ProgressUpdateDto
        {
            MemberId = member.Id,
            MemberNickname = member.Nickname,
            GoalId = progress.GoalId,
            GoalTitle = progress.Goal?.Title ?? "",
            GoalType = progress.Goal?.Type ?? GoalType.Boolean,
            CurrentValue = progress.CurrentValue,
            TargetValue = progress.Goal?.TargetValue ?? 1,
            IsCompleted = progress.IsCompleted,
            PercentageComplete = progress.PercentageComplete
        };

        // Odeslání studentům i učiteli
        await _hubContext.Clients.Group(groupId.ToString()).ProgressUpdated(dto);
        await _hubContext.Clients.Group($"teacher-{groupId}").ProgressUpdated(dto);
    }

    /// <inheritdoc />
    public async Task NotifyMemberJoined(Guid groupId, GroupMember member)
    {
        var dto = MemberDto.FromEntity(member);
        // Pouze učitel potřebuje vědět o připojení
        await _hubContext.Clients.Group($"teacher-{groupId}").MemberJoined(dto);
    }

    /// <inheritdoc />
    public async Task NotifyMemberNeedsHelp(Guid groupId, HelpRequest request, string memberNickname)
    {
        var dto = new HelpRequestDto
        {
            Id = request.Id,
            MemberId = request.MemberId,
            MemberNickname = memberNickname,
            Reason = request.Reason,
            Type = request.Type,
            CreatedAt = request.CreatedAt
        };

        // Pouze učitel potřebuje vědět o žádosti o pomoc
        await _hubContext.Clients.Group($"teacher-{groupId}").MemberNeedsHelp(dto);
    }

    /// <inheritdoc />
    public async Task NotifyHelpRequested(Guid groupId, Guid memberId, string reason)
    {
        // Vytvoření zjednodušeného DTO pro manuální žádost
        var dto = new HelpRequestDto
        {
            Id = Guid.NewGuid(),
            MemberId = memberId,
            MemberNickname = "",
            Reason = reason,
            Type = HelpRequestType.Manual,
            CreatedAt = DateTime.UtcNow
        };

        await _hubContext.Clients.Group($"teacher-{groupId}").MemberNeedsHelp(dto);
    }

    /// <inheritdoc />
    public async Task NotifyHelpResolved(Guid groupId, Guid memberId)
    {
        await _hubContext.Clients.Group($"teacher-{groupId}").HelpResolved(memberId);
    }

    /// <inheritdoc />
    public async Task NotifyMemberActivityChanged(Guid groupId, Guid memberId, bool isActive)
    {
        await _hubContext.Clients.Group($"teacher-{groupId}").MemberActivityChanged(memberId, isActive);
    }
}

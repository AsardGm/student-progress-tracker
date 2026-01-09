using Microsoft.AspNetCore.SignalR;
using StudentProgressTracker.Models;

namespace StudentProgressTracker.Hubs;

/// <summary>
/// SignalR hub pro real-time komunikaci mezi serverem a klienty.
/// Zajišťuje synchronizaci pokroku, chatových zpráv a notifikací o pomoci.
/// Používá strongly-typed klienty přes IProgressHubClient.
/// </summary>
public class ProgressHub : Hub<IProgressHubClient>
{
    /// <summary>
    /// Přidá klienta do SignalR skupiny pro danou studijní skupinu.
    /// Používá se pro studenty připojené do skupiny.
    /// </summary>
    /// <param name="groupId">ID studijní skupiny.</param>
    public async Task JoinGroup(string groupId)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupId);
    }

    /// <summary>
    /// Odebere klienta ze SignalR skupiny.
    /// </summary>
    /// <param name="groupId">ID studijní skupiny.</param>
    public async Task LeaveGroup(string groupId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupId);
    }

    /// <summary>
    /// Přidá klienta do učitelské skupiny pro danou studijní skupinu.
    /// Učitelé přijímají notifikace o pokroku a žádostech o pomoc.
    /// </summary>
    /// <param name="groupId">ID studijní skupiny.</param>
    public async Task JoinTeacherDashboard(string groupId)
    {
        // Učitelé mají oddělený kanál s prefixem "teacher-"
        await Groups.AddToGroupAsync(Context.ConnectionId, $"teacher-{groupId}");
    }

    /// <summary>
    /// Odebere klienta z učitelské skupiny.
    /// </summary>
    /// <param name="groupId">ID studijní skupiny.</param>
    public async Task LeaveTeacherDashboard(string groupId)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"teacher-{groupId}");
    }
}

/// <summary>
/// Rozhraní definující metody volatelné na klientech přes SignalR.
/// Strongly-typed alternativa k Clients.All.SendAsync().
/// </summary>
public interface IProgressHubClient
{
    /// <summary>
    /// Přijímá novou chatovou zprávu.
    /// </summary>
    Task ReceiveMessage(ChatMessageDto message);

    /// <summary>
    /// Přijímá aktualizaci pokroku studenta.
    /// </summary>
    Task ProgressUpdated(ProgressUpdateDto update);

    /// <summary>
    /// Přijímá informaci o připojení nového člena (pouze učitel).
    /// </summary>
    Task MemberJoined(MemberDto member);

    /// <summary>
    /// Přijímá žádost studenta o pomoc (pouze učitel).
    /// </summary>
    Task MemberNeedsHelp(HelpRequestDto request);

    /// <summary>
    /// Přijímá informaci o vyřešení žádosti o pomoc (pouze učitel).
    /// </summary>
    Task HelpResolved(Guid memberId);

    /// <summary>
    /// Přijímá změnu stavu aktivity studenta (pouze učitel).
    /// </summary>
    Task MemberActivityChanged(Guid memberId, bool isActive);
}

/// <summary>
/// Data transfer object pro chatové zprávy přenášené přes SignalR.
/// Obsahuje pouze data potřebná pro zobrazení v UI.
/// </summary>
public class ChatMessageDto
{
    /// <summary>ID zprávy.</summary>
    public Guid Id { get; set; }

    /// <summary>ID skupiny.</summary>
    public Guid GroupId { get; set; }

    /// <summary>ID autora zprávy (null pro systémové zprávy).</summary>
    public Guid? MemberId { get; set; }

    /// <summary>Přezdívka autora zprávy.</summary>
    public string? MemberNickname { get; set; }

    /// <summary>Textový obsah zprávy.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Typ zprávy (User/System/AI).</summary>
    public MessageType Type { get; set; }

    /// <summary>Indikuje relevanci k učebním cílům.</summary>
    public bool IsRelevantToGoal { get; set; }

    /// <summary>Indikuje varování (off-topic apod.).</summary>
    public bool IsWarning { get; set; }

    /// <summary>Čas odeslání zprávy.</summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Vytvoří DTO z entity ChatMessage.
    /// </summary>
    /// <param name="entity">Entity z databáze.</param>
    /// <returns>DTO pro přenos přes SignalR.</returns>
    public static ChatMessageDto FromEntity(ChatMessage entity)
    {
        return new ChatMessageDto
        {
            Id = entity.Id,
            GroupId = entity.GroupId,
            MemberId = entity.MemberId,
            MemberNickname = entity.Member?.Nickname,
            Content = entity.Content,
            Type = entity.Type,
            IsRelevantToGoal = entity.IsRelevantToGoal,
            IsWarning = entity.IsWarning,
            Timestamp = entity.Timestamp
        };
    }
}

/// <summary>
/// Data transfer object pro aktualizace pokroku přenášené přes SignalR.
/// </summary>
public class ProgressUpdateDto
{
    /// <summary>ID člena (studenta).</summary>
    public Guid MemberId { get; set; }

    /// <summary>Přezdívka studenta.</summary>
    public string MemberNickname { get; set; } = string.Empty;

    /// <summary>ID cíle.</summary>
    public Guid GoalId { get; set; }

    /// <summary>Název cíle.</summary>
    public string GoalTitle { get; set; } = string.Empty;

    /// <summary>Typ cíle (Boolean/Percentage).</summary>
    public GoalType GoalType { get; set; }

    /// <summary>Aktuální hodnota pokroku.</summary>
    public int CurrentValue { get; set; }

    /// <summary>Cílová hodnota.</summary>
    public int TargetValue { get; set; }

    /// <summary>Indikuje splnění cíle.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>Procentuální dokončení (0-100).</summary>
    public int PercentageComplete { get; set; }
}

/// <summary>
/// Data transfer object pro člena skupiny přenášený přes SignalR.
/// </summary>
public class MemberDto
{
    /// <summary>ID člena.</summary>
    public Guid Id { get; set; }

    /// <summary>Přezdívka člena.</summary>
    public string Nickname { get; set; } = string.Empty;

    /// <summary>Čas připojení do skupiny.</summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>Indikuje potřebu pomoci.</summary>
    public bool NeedsHelp { get; set; }

    /// <summary>Seznam pokroků u jednotlivých cílů.</summary>
    public List<ProgressUpdateDto> Progresses { get; set; } = new();

    /// <summary>
    /// Vytvoří DTO z entity GroupMember.
    /// </summary>
    /// <param name="entity">Entity z databáze.</param>
    /// <returns>DTO pro přenos přes SignalR.</returns>
    public static MemberDto FromEntity(GroupMember entity)
    {
        return new MemberDto
        {
            Id = entity.Id,
            Nickname = entity.Nickname,
            JoinedAt = entity.JoinedAt,
            NeedsHelp = entity.NeedsHelp,
            Progresses = entity.Progresses.Select(p => new ProgressUpdateDto
            {
                MemberId = entity.Id,
                MemberNickname = entity.Nickname,
                GoalId = p.GoalId,
                GoalTitle = p.Goal?.Title ?? "",
                GoalType = p.Goal?.Type ?? GoalType.Boolean,
                CurrentValue = p.CurrentValue,
                TargetValue = p.Goal?.TargetValue ?? 1,
                IsCompleted = p.IsCompleted,
                PercentageComplete = p.PercentageComplete
            }).ToList()
        };
    }
}

/// <summary>
/// Data transfer object pro žádosti o pomoc přenášené přes SignalR.
/// </summary>
public class HelpRequestDto
{
    /// <summary>ID žádosti.</summary>
    public Guid Id { get; set; }

    /// <summary>ID člena žádajícího o pomoc.</summary>
    public Guid MemberId { get; set; }

    /// <summary>Přezdívka člena.</summary>
    public string MemberNickname { get; set; } = string.Empty;

    /// <summary>Důvod žádosti o pomoc.</summary>
    public string Reason { get; set; } = string.Empty;

    /// <summary>Typ žádosti (Manual/Inactivity/OffTopic/AIDetected).</summary>
    public HelpRequestType Type { get; set; }

    /// <summary>Čas vytvoření žádosti.</summary>
    public DateTime CreatedAt { get; set; }
}

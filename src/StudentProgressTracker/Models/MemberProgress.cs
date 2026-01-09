namespace StudentProgressTracker.Models;

/// <summary>
/// Reprezentuje pokrok konkrétního studenta u konkrétního cíle.
/// Propojuje GroupMember s Goal a sleduje aktuální hodnotu pokroku.
/// </summary>
public class MemberProgress
{
    /// <summary>
    /// Unikátní identifikátor záznamu pokroku.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Aktuální hodnota pokroku.
    /// U binárních cílů: 0 nebo 1.
    /// U procentuálních cílů: 0 až TargetValue.
    /// </summary>
    public int CurrentValue { get; set; }

    /// <summary>
    /// Indikuje, zda byl cíl splněn.
    /// True pokud CurrentValue >= TargetValue.
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Datum a čas poslední aktualizace pokroku (UTC).
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ID člena (studenta), kterému pokrok patří.
    /// </summary>
    public Guid MemberId { get; set; }

    /// <summary>
    /// ID cíle, ke kterému se pokrok vztahuje.
    /// </summary>
    public Guid GoalId { get; set; }

    /// <summary>
    /// Navigační vlastnost na člena skupiny.
    /// </summary>
    public GroupMember Member { get; set; } = null!;

    /// <summary>
    /// Navigační vlastnost na cíl.
    /// </summary>
    public Goal Goal { get; set; } = null!;

    /// <summary>
    /// Vypočítaná vlastnost vracející procentuální dokončení cíle (0-100).
    /// U binárních cílů vrací 0 nebo 100.
    /// U procentuálních cílů vrací poměr CurrentValue/TargetValue.
    /// </summary>
    public int PercentageComplete => Goal?.Type == GoalType.Percentage && Goal.TargetValue > 0
        ? (int)Math.Round((double)CurrentValue / Goal.TargetValue * 100)
        : (IsCompleted ? 100 : 0);
}

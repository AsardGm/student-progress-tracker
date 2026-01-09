namespace StudentProgressTracker.Models;

/// <summary>
/// Reprezentuje učební cíl definovaný učitelem pro skupinu.
/// Cíle mohou být binární (splněno/nesplněno) nebo procentuální (progress bar).
/// </summary>
public class Goal
{
    /// <summary>
    /// Unikátní identifikátor cíle.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Název cíle zobrazovaný studentům i učiteli.
    /// Např. "Vyřešit kvadratickou rovnici".
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Podrobný popis cíle pro AI analýzu.
    /// AI používá tento popis k hodnocení relevance zpráv.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Typ cíle určující způsob zobrazení pokroku.
    /// Boolean = checkbox, Percentage = progress bar.
    /// </summary>
    public GoalType Type { get; set; } = GoalType.Boolean;

    /// <summary>
    /// Cílová hodnota pro procentuální cíle.
    /// Např. 3 pro "vyřešit 3 rovnice" (progress 0/3, 1/3, 2/3, 3/3).
    /// U binárních cílů je hodnota 1.
    /// </summary>
    public int TargetValue { get; set; } = 1;

    /// <summary>
    /// Pořadí cíle pro zobrazení v seznamu.
    /// Nižší číslo = vyšší priorita.
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// ID skupiny, ke které cíl patří.
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// Navigační vlastnost na skupinu.
    /// </summary>
    public Group Group { get; set; } = null!;

    /// <summary>
    /// Kolekce pokroků jednotlivých členů u tohoto cíle.
    /// </summary>
    public ICollection<MemberProgress> MemberProgresses { get; set; } = new List<MemberProgress>();
}

/// <summary>
/// Definuje typ cíle a způsob zobrazení pokroku.
/// </summary>
public enum GoalType
{
    /// <summary>
    /// Binární cíl - zobrazuje se jako checkbox (splněno/nesplněno).
    /// </summary>
    Boolean,

    /// <summary>
    /// Procentuální cíl - zobrazuje se jako progress bar (0-100%).
    /// </summary>
    Percentage
}
